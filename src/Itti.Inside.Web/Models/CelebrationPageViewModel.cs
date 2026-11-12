using Itti.Inside.Domain.Content;

namespace Itti.Inside.Web.Models;

/// <summary>The full Wedding Bells or New Born page.</summary>
/// <param name="Title">The page heading.</param>
/// <param name="Art">The picture name in <c>wwwroot/images/art</c>, as on the landing page card.</param>
/// <param name="Board">Every announcement, newest posted first, and the closing line.</param>
public sealed record CelebrationPageViewModel(string Title, string Art, CelebrationBoard Board);
