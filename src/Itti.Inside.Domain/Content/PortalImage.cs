namespace Itti.Inside.Domain.Content;

/// <summary>
/// A picture the admin uploads for a CEO's Desk message or a Rockstar Award. It lives in its own table,
/// so lists of messages and awards load without the picture data, and a page asks for each picture by Id.
/// </summary>
public sealed class PortalImage
{
    public int Id { get; init; }

    public required string ContentType { get; init; }

    public required byte[] Content { get; init; }

    public required DateTimeOffset UploadedOn { get; init; }
}

/// <summary>Content that can have one picture.</summary>
public interface IHasImage
{
    int? ImageId { get; }
}

public static class PortalImageRules
{
    public const int MaxBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Photos only. SVG is left out on purpose: it can carry script, and these pictures come from an upload.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ContentTypes { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
    };
}
