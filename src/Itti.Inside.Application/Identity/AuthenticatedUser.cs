namespace Itti.Inside.Application.Identity;

public sealed record AuthenticatedUser(string UserId, string DisplayName, string Email, IReadOnlyList<string> Roles);
