using System.Net;
using System.Text.RegularExpressions;

namespace Itti.Inside.Tests;

/// <summary>Finds a link in a page by its visible text, whatever order its attributes are in.</summary>
public static partial class PageLinks
{
    public static PageLink Find(string html, string text)
    {
        var link = All(html).FirstOrDefault(candidate => candidate.Text == text);

        Assert.True(link is not null, $"No link with the text '{text}'.");

        return link;
    }

    /// <summary>Every link in the page, in order.</summary>
    public static IReadOnlyList<PageLink> All(string html)
    {
        ArgumentNullException.ThrowIfNull(html);

        return Anchor().Matches(html)
            .Select(match => new PageLink(
                match.Groups["tag"].Value,
                WebUtility.HtmlDecode(Href().Match(match.Groups["tag"].Value).Groups[1].Value),
                WebUtility.HtmlDecode(Markup().Replace(match.Groups["text"].Value, string.Empty)).Trim()))
            .ToList();
    }

    [GeneratedRegex("(?<tag><a\\s[^>]*>)(?<text>.*?)</a>", RegexOptions.Singleline)]
    private static partial Regex Anchor();

    [GeneratedRegex("href=\"([^\"]*)\"")]
    private static partial Regex Href();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Markup();
}

/// <param name="Tag">The opening tag with all its attributes.</param>
/// <param name="Href">The address, decoded.</param>
/// <param name="Text">The visible text, without icons or other markup.</param>
public sealed record PageLink(string Tag, string Href, string Text)
{
    public bool OpensInNewTab =>
        Tag.Contains("target=\"_blank\"", StringComparison.Ordinal)
        && Tag.Contains("rel=\"noopener\"", StringComparison.Ordinal);
}
