using System.Net;
using System.Text.RegularExpressions;

namespace Itti.Inside.Tests;

/// <summary>Drives the portal the way a browser does, including the anti-forgery token.</summary>
public static partial class BrowserSession
{
    public static async Task<HttpResponseMessage> PostSignInAsync(
        this HttpClient client,
        string userId,
        string password,
        string? returnUrl = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        var token = await client.GetAntiforgeryTokenAsync("/account/login");

        return await client.PostAsync(new Uri("/account/login", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["UserId"] = userId,
            ["Password"] = password,
            ["ReturnUrl"] = returnUrl ?? string.Empty,
        }));
    }

    /// <summary>Signs in with the employee test account.</summary>
    public static async Task SignInAsync(this HttpClient client)
    {
        using var response = await client.PostSignInAsync(PortalWebFactory.UserId, PortalWebFactory.Password);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    /// <summary>Signs in with the admin test account, on the same sign-in page.</summary>
    public static async Task SignInAsAdminAsync(this HttpClient client)
    {
        using var response = await client.PostSignInAsync(PortalWebFactory.AdminId, PortalWebFactory.AdminPassword);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    /// <summary>Opens the page that holds a form, then posts the form with that page's token.</summary>
    public static async Task<HttpResponseMessage> PostFormAsync(
        this HttpClient client,
        string pagePath,
        string actionPath,
        params KeyValuePair<string, string>[] fields)
    {
        ArgumentNullException.ThrowIfNull(client);

        var token = await client.GetAntiforgeryTokenAsync(pagePath);

        return await client.PostAsync(
            new Uri(actionPath, UriKind.Relative),
            new FormUrlEncodedContent(fields.Append(new("__RequestVerificationToken", token))));
    }

    public static async Task<string> GetAntiforgeryTokenAsync(this HttpClient client, string path)
    {
        ArgumentNullException.ThrowIfNull(client);

        var html = await client.GetStringAsync(new Uri(path, UriKind.Relative));
        var match = AntiforgeryField().Match(html);

        Assert.True(match.Success, $"No anti-forgery token on {path}.");

        return match.Groups[1].Value;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryField();
}
