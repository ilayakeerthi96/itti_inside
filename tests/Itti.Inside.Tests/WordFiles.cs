using System.IO.Compression;
using System.Security;
using System.Text;

namespace Itti.Inside.Tests;

/// <summary>Builds small Word (.docx) files with tables, shaped like HR's holiday list.</summary>
public static class WordFiles
{
    public static byte[] WithTables(params string[][][] tables)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var body = new StringBuilder();
        body.Append("<w:p><w:r><w:t>LIST OF HOLIDAYS</w:t></w:r></w:p>");

        foreach (var table in tables)
        {
            body.Append("<w:tbl>");
            foreach (var row in table)
            {
                body.Append("<w:tr>");
                foreach (var cell in row)
                {
                    body.Append("<w:tc><w:p><w:r><w:t xml:space=\"preserve\">")
                        .Append(SecurityElement.Escape(cell))
                        .Append("</w:t></w:r></w:p></w:tc>");
                }

                body.Append("</w:tr>");
            }

            body.Append("</w:tbl>");
        }

        var document =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
            + "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>"
            + body
            + "</w:body></w:document>";

        using var memory = new MemoryStream();
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var writer = new StreamWriter(archive.CreateEntry("word/document.xml").Open(), new UTF8Encoding(false));
            writer.Write(document);
        }

        return memory.ToArray();
    }

    /// <summary>The first table of "ITTI_HOLIDAY LIST - 2026_1.0.docx", shortened, with its quirks kept.</summary>
    public static byte[] HolidayList2026() => WithTables(
    [
        ["SL #", "PUBLIC HOLIDAY", "DATE", "DAY"],
        ["1", "NEW YEAR", "01-01-2026", "THURSDAY"],
        ["4", "HOLI* (Optional)", "04-03-2026", "WEDNESDAY"],
        ["5", "UGADI / GUDI PADWA", "19-03-2026", "THURSDAY"],
        ["7", "MAY DAY (International worker’s Day)", "01-05-2026", "FRIDAY"],
        ["14", "KANNADA RAJYOTSAVA/ NARAKA CHATURDASHI", "01/11/2026", "SUNDAY"],
        ["", "", "", ""],
    ],
    [
        ["SL #", "PUBLIC HOLIDAY", "DATE", "DAY"],
        ["1", "FROM THE SECOND TABLE", "02-02-2026", "MONDAY"],
    ]);
}
