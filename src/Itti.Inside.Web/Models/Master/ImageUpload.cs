using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models.Master;

/// <summary>A master page form whose item can have a picture.</summary>
public interface IImageForm
{
    /// <summary>The current picture, shown on the form while editing.</summary>
    int? ImageId { get; set; }

    /// <summary>A new picture to upload, replacing the current one.</summary>
    IFormFile? Image { get; set; }

    /// <summary>Ticked to remove the current picture.</summary>
    bool RemoveImage { get; set; }
}

/// <summary>Checks a picture the admin uploads before it is stored and shown to employees.</summary>
public static class ImageUpload
{
    public const string Accept = ".jpg,.jpeg,.png,.webp";

    public static async Task<(PortalImage? Image, string? Problem)> ReadAsync(
        IFormFile file,
        DateTimeOffset uploadedOn,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length > PortalImageRules.MaxBytes)
        {
            return (null, "The picture is larger than 5 MB. Choose a smaller one.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!PortalImageRules.ContentTypes.TryGetValue(extension, out var contentType))
        {
            return (null, "Choose a JPG, PNG or WebP picture.");
        }

        using var memory = new MemoryStream();
        await file.CopyToAsync(memory, cancellationToken);
        var content = memory.ToArray();

        // The name could say .png about anything, so check that the content really is that kind of picture.
        if (!StartsLike(contentType, content))
        {
            return (null, "This file isn't a real picture. Choose a JPG, PNG or WebP picture.");
        }

        return (new PortalImage { ContentType = contentType, Content = content, UploadedOn = uploadedOn }, null);
    }

    private static bool StartsLike(string contentType, byte[] content) => contentType switch
    {
        "image/jpeg" => content is [0xFF, 0xD8, 0xFF, ..],
        "image/png" => content is [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, ..],
        "image/webp" => content is [(byte)'R', (byte)'I', (byte)'F', (byte)'F', _, _, _, _, (byte)'W', (byte)'E', (byte)'B', (byte)'P', ..],
        _ => false,
    };
}
