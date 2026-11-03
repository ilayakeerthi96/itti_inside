namespace Itti.Inside.Web.Models;

public static class PersonDisplay
{
    /// <summary>Up to two initials for an avatar, for example "Satyajit Das" becomes "SD".</summary>
    public static string Initials(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            0 => string.Empty,
            1 => parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(parts[0][..1], parts[^1][..1]).ToUpperInvariant(),
        };
    }
}
