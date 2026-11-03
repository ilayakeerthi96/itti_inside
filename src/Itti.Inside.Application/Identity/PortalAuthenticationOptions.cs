using System.ComponentModel.DataAnnotations;

namespace Itti.Inside.Application.Identity;

/// <summary>Bound from the <c>Authentication</c> configuration section.</summary>
public sealed class PortalAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public const string DemoProvider = "Demo";

    /// <summary>Which <c>ICredentialValidator</c> to use.</summary>
    [Required]
    public string Provider { get; set; } = DemoProvider;

    /// <summary>How long an idle session stays signed in.</summary>
    [Range(5, 1440)]
    public int SessionMinutes { get; set; } = 480;

    /// <summary>The employee account of the Demo provider.</summary>
    public DemoUserOptions Demo { get; set; } = new();

    /// <summary>The admin account of the Demo provider. It works only on the admin sign-in page.</summary>
    public DemoUserOptions Admin { get; set; } = new() { DisplayName = "Portal Admin", Email = "portal.admin@example.com" };
}

public sealed class DemoUserOptions
{
    public string UserId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = "Demo User";

    public string Email { get; set; } = "demo.user@example.com";
}
