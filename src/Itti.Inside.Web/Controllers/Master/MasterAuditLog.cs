using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Controllers.Master;

/// <summary>Records who changed the main page.</summary>
internal static partial class MasterAuditLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} saved {Section} item {ItemId}.")]
    public static partial void ItemSaved(this ILogger logger, string? userId, string section, int itemId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} deleted {Section} item {ItemId}.")]
    public static partial void ItemDeleted(this ILogger logger, string? userId, string section, int itemId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} changed the text {TextKey}.")]
    public static partial void TextSaved(this ILogger logger, string? userId, PortalTextKey textKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} uploaded {FileName} ({Bytes} bytes) as {DocumentKey}.")]
    public static partial void DocumentUploaded(this ILogger logger, string? userId, string fileName, int bytes, string documentKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} removed the file {DocumentKey}.")]
    public static partial void DocumentDeleted(this ILogger logger, string? userId, string documentKey);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} replaced the holidays from a file: {Count} holidays.")]
    public static partial void HolidaysImported(this ILogger logger, string? userId, int count);
}
