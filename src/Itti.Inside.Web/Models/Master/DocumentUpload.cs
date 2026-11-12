using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

/// <summary>Checks a file the admin uploads before it is stored and offered to employees.</summary>
public static class DocumentUpload
{
    /// <summary>The upload limit for a request, a little above the file limit to leave room for the form.</summary>
    public const long RequestLimit = PortalDocumentKeys.MaxBytes + (512 * 1024);

    public static async Task<(PortalDocument? Document, string? Problem)> ReadAsync(
        IFormFile? file,
        string key,
        IReadOnlyCollection<string> allowedExtensions,
        DateTimeOffset uploadedOn,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(allowedExtensions);

        var allowed = string.Join(", ", allowedExtensions.Select(extension => extension.TrimStart('.').ToUpperInvariant()));

        if (file is null || file.Length == 0)
        {
            return (null, "Nothing was uploaded. Choose a file first.");
        }

        if (file.Length > PortalDocumentKeys.MaxBytes)
        {
            return (null, "Nothing was uploaded. The file is larger than 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return (null, $"Nothing was uploaded. Choose a {allowed} file.");
        }

        using var memory = new MemoryStream();
        await file.CopyToAsync(memory, cancellationToken);
        var content = memory.ToArray();

        // The name could say .pdf about anything, so check that the content starts like one.
        if (!StartsLike(extension, content))
        {
            return (null, $"Nothing was uploaded. The file isn't a real {extension.TrimStart('.').ToUpperInvariant()} file.");
        }

        return (new PortalDocument
        {
            Key = key,
            FileName = SafeFileName(file.FileName, extension),
            ContentType = PortalDocumentKeys.ContentTypes[extension],
            Content = content,
            UploadedOn = uploadedOn,
        }, null);
    }

    public static bool IsWordFile(PortalDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);
    }

    // PDF files start with "%PDF". Word and Excel files are zip archives, which start with "PK".
    private static bool StartsLike(string extension, byte[] content) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            ? content is [(byte)'%', (byte)'P', (byte)'D', (byte)'F', ..]
            : content is [(byte)'P', (byte)'K', ..];

    // Keeps the name people recognise, without folders or characters that break a download.
    private static string SafeFileName(string fileName, string extension)
    {
        var name = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Where(character => !invalid.Contains(character) && !char.IsControl(character)).ToArray()).Trim();

        if (clean.Length == 0)
        {
            clean = "document";
        }

        var maxLength = ContentLimits.ShortText - extension.Length;

        return (clean.Length > maxLength ? clean[..maxLength] : clean) + extension.ToLowerInvariant();
    }
}
