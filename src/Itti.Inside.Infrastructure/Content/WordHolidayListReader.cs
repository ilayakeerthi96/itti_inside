using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;

namespace Itti.Inside.Infrastructure.Content;

/// <summary>
/// Reads a holiday list from a Word (.docx) file, such as the one HR sends out each year. It takes the
/// first table that has a date column, for example "SL # | PUBLIC HOLIDAY | DATE | DAY". A holiday marked
/// "Optional" or with an asterisk becomes an optional holiday.
/// </summary>
public sealed partial class WordHolidayListReader : IHolidayListReader
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    // A real holiday list is a few kilobytes. The limit stops a crafted file from unpacking into gigabytes.
    private const long MaxDocumentXmlBytes = 5 * 1024 * 1024;

    private static readonly string[] DateFormats =
    [
        "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/M/yyyy", "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd",
        "dd-MMM-yyyy", "d-MMM-yyyy", "d MMM yyyy", "d MMMM yyyy", "dd MMMM yyyy", "MMMM d, yyyy",
    ];

    public IReadOnlyList<Holiday> Read(Stream wordDocument)
    {
        ArgumentNullException.ThrowIfNull(wordDocument);

        XDocument document;
        try
        {
            document = LoadDocumentXml(wordDocument);
        }
        catch (Exception exception) when (exception is InvalidDataException or XmlException or IOException)
        {
            // Not a Word file, or a damaged one.
            return [];
        }

        foreach (var table in document.Descendants(W + "tbl"))
        {
            var rows = table.Elements(W + "tr").Select(Cells).ToList();
            var header = rows.FirstOrDefault();
            var dateColumn = header?.FindIndex(cell => cell.Contains("DATE", StringComparison.OrdinalIgnoreCase)) ?? -1;

            if (dateColumn < 0)
            {
                continue;
            }

            var nameColumn = header!.FindIndex(cell =>
                cell.Contains("HOLIDAY", StringComparison.OrdinalIgnoreCase)
                || cell.Contains("OCCASION", StringComparison.OrdinalIgnoreCase)
                || cell.Contains("FESTIVAL", StringComparison.OrdinalIgnoreCase)
                || cell.Contains("NAME", StringComparison.OrdinalIgnoreCase));

            var holidays = rows.Skip(1)
                .Select(cells => ToHoliday(cells, dateColumn, nameColumn))
                .OfType<Holiday>()
                .ToList();

            if (holidays.Count > 0)
            {
                return holidays;
            }
        }

        return [];
    }

    private static XDocument LoadDocumentXml(Stream wordDocument)
    {
        using var archive = new ZipArchive(wordDocument, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.GetEntry("word/document.xml") ?? throw new InvalidDataException("No word/document.xml.");

        if (entry.Length > MaxDocumentXmlBytes)
        {
            throw new InvalidDataException("The document is too large.");
        }

        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });

        return XDocument.Load(reader);
    }

    private static List<string> Cells(XElement row) =>
        row.Elements(W + "tc")
            .Select(cell => string.Concat(cell.Descendants(W + "t").Select(text => text.Value)).Trim())
            .ToList();

    private static Holiday? ToHoliday(List<string> cells, int dateColumn, int nameColumn)
    {
        if (dateColumn >= cells.Count
            || !DateOnly.TryParseExact(cells[dateColumn], DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date))
        {
            return null;
        }

        // Without a named column, the name is the first cell that is neither the number, the date nor the day.
        var rawName = nameColumn >= 0 && nameColumn < cells.Count
            ? cells[nameColumn]
            : cells.Where((cell, index) => index != dateColumn && !int.TryParse(cell, out _) && !IsDayName(cell)).FirstOrDefault() ?? string.Empty;

        var isOptional = rawName.Contains('*', StringComparison.Ordinal) || rawName.Contains("optional", StringComparison.OrdinalIgnoreCase);
        var name = Tidy(OptionalMark().Replace(rawName, " "));

        return name.Length == 0 ? null : new Holiday(0, date, name, isOptional);
    }

    private static bool IsDayName(string cell) =>
        Enum.TryParse<DayOfWeek>(cell, ignoreCase: true, out _);

    // "UGADI / GUDI PADWA" becomes "Ugadi / Gudi Padwa", with spaces tidied and the length capped.
    private static string Tidy(string name)
    {
        var words = Spaces().Replace(name, " ").Trim().ToLowerInvariant();
        var titled = WordStart().Replace(words, match => match.Value.ToUpperInvariant());

        return titled.Length > ContentLimits.ShortText ? titled[..ContentLimits.ShortText] : titled;
    }

    [GeneratedRegex(@"\*|\(\s*optional\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex OptionalMark();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"(?<![\p{L}’'])\p{L}")]
    private static partial Regex WordStart();
}
