namespace Itti.Inside.Web.Models;

/// <summary>Turns text typed on the master page into what the portal pages show.</summary>
public static class TextDisplay
{
    /// <summary>The non-empty lines, for text entered as one item per line.</summary>
    public static IReadOnlyList<string> Lines(string? text) =>
        (text ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

    /// <summary>The start of a long text, cut at a word and ending in an ellipsis when it is longer.</summary>
    public static string Shorten(string? text, int maxLength)
    {
        var value = (text ?? string.Empty).Trim();

        if (value.Length <= maxLength)
        {
            return value;
        }

        var cut = value.LastIndexOf(' ', maxLength);

        return string.Concat(value.AsSpan(0, cut > maxLength / 2 ? cut : maxLength).TrimEnd(" ,.;:"), "…");
    }
}

/// <summary>How the portal pages show addresses that the admin types in.</summary>
public static class LinkDisplay
{
    public static bool IsWebAddress(string? text) =>
        Uri.TryCreate(text, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    public static bool IsEmailAddress(string? text) =>
        !string.IsNullOrWhiteSpace(text)
        && text.Count(character => character == '@') == 1
        && !text.Contains(' ', StringComparison.Ordinal)
        && text.IndexOf('@', StringComparison.Ordinal) > 0
        && text.IndexOf('.', text.IndexOf('@', StringComparison.Ordinal)) > 0;
}
