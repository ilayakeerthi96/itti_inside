using Itti.Inside.Application.Identity;

namespace Itti.Inside.Application.Abstractions;

/// <summary>
/// Checks a user ID and password. Replace the demo implementation with Active Directory,
/// LDAP or your HR database. Sign-in, cookies and authorization do not change.
/// </summary>
public interface ICredentialValidator
{
    /// <returns>The signed-in user, or <see langword="null"/> when the credentials are wrong.</returns>
    Task<AuthenticatedUser?> ValidateAsync(string userId, string password, CancellationToken cancellationToken);
}
