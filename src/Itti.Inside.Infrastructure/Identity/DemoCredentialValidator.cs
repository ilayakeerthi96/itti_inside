using System.Security.Cryptography;
using System.Text;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Application.Identity;
using Microsoft.Extensions.Options;

namespace Itti.Inside.Infrastructure.Identity;

/// <summary>
/// Dummy sign-in that checks one employee and one admin account from configuration. It exists for the
/// prototype only: <see cref="DependencyInjection.AddInfrastructure"/> refuses to start with it in Production.
/// </summary>
public sealed class DemoCredentialValidator(IOptions<PortalAuthenticationOptions> options) : ICredentialValidator
{
    private static readonly string[] EmployeeRoles = [PortalRoles.Employee];
    private static readonly string[] AdminRoles = [PortalRoles.Admin];

    public Task<AuthenticatedUser?> ValidateAsync(string userId, string password, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(password);

        var accounts = options.Value;

        var user = Match(accounts.Demo, userId, password, EmployeeRoles)
            ?? Match(accounts.Admin, userId, password, AdminRoles);

        return Task.FromResult(user);
    }

    private static AuthenticatedUser? Match(DemoUserOptions account, string userId, string password, string[] roles)
    {
        // An account without a user ID is switched off.
        if (account.UserId.Length == 0)
        {
            return null;
        }

        var userMatches = string.Equals(userId.Trim(), account.UserId, StringComparison.OrdinalIgnoreCase);
        var passwordMatches = HashesMatch(password, account.Password);

        return userMatches && passwordMatches
            ? new AuthenticatedUser(account.UserId, account.DisplayName, account.Email, roles)
            : null;
    }

    // Hashing first gives both sides the same length, so the comparison stays constant time.
    private static bool HashesMatch(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(left)),
            SHA256.HashData(Encoding.UTF8.GetBytes(right)));
}
