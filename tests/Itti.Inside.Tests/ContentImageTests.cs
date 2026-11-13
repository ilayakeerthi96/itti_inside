using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itti.Inside.Tests;

/// <summary>Pictures on CEO's Desk messages and Rockstar Awards. Own database, because it adds and removes pictures.</summary>
public sealed partial class ContentImageTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    // Only the start of a file is checked, so a PNG signature and a few bytes stand in for a photo.
    private static readonly byte[] Png = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 5, 6, 7, 8];

    [Fact]
    public async Task Ceo_message_picture_shows_on_the_card_and_the_ceo_page()
    {
        using var admin = await AdminAsync();

        using var saved = await PostAsync(admin, "/master/ceo-desk", ("photo.png", Png),
            ("Form.Title", "Message with a picture"), ("Form.PostedOn", Day(0)), ("Form.Message", "Hello with a picture."));

        var home = await PageAsync("/");
        var page = await PageAsync("/ceo-desk");
        var imageId = ImageIdIn(Section(home, "ceo-desk"));

        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.Contains($"/media/{imageId}", page, StringComparison.Ordinal);
        Assert.Contains("alt=\"Picture for Message with a picture\"", page, StringComparison.Ordinal);

        using var visitor = factory.CreateBrowser();
        using var image = await visitor.GetAsync(new Uri($"/media/{imageId}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType?.MediaType);
        Assert.Equal(Png, await image.Content.ReadAsByteArrayAsync());
        Assert.True(image.Headers.CacheControl?.MaxAge > TimeSpan.FromDays(30));
    }

    [Fact]
    public async Task Award_pictures_show_on_the_card_and_the_awards_page()
    {
        using var admin = await AdminAsync();

        await PostAwardAsync(admin, [("winner.jpg", Jpeg), ("team.png", Png), ("stage.png", Png)],
            ("Form.Title", "Award with pictures"), ("Form.PublishedOn", "2031-01-01"), ("Form.Excerpt", "Well done."));

        var card = Award(await PageAsync("/"), "Award with pictures");
        var page = Award(await PageAsync("/rockstar-awards"), "Award with pictures");

        Assert.Contains("class=\"award__image\"", card, StringComparison.Ordinal);
        Assert.DoesNotContain("award__star", card, StringComparison.Ordinal);
        Assert.Contains("+2 more", card, StringComparison.Ordinal);
        Assert.Equal(3, ImageIdsIn(card).Count);
        Assert.Equal(ImageIdsIn(card), ImageIdsIn(page));
        Assert.Contains("award__strip", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Every_award_picture_opens_in_the_preview()
    {
        using var admin = await AdminAsync();
        await PostAwardAsync(admin, [("one.png", Png), ("two.png", Png)],
            ("Form.Title", "Award to preview"), ("Form.PublishedOn", "2031-01-02"), ("Form.Excerpt", "Text."));

        var card = Award(await PageAsync("/"), "Award to preview");

        // The cover is the button. The second picture waits, hidden, for the preview to page to it.
        Assert.Equal(2, PreviewPhoto().Count(card));
        Assert.Contains("<button type=\"button\" class=\"photo-button award__cover\" data-photo=", card, StringComparison.Ordinal);
        Assert.Contains("<span hidden data-photo=", card, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Adding_keeps_the_current_pictures_and_removing_deletes_them()
    {
        using var admin = await AdminAsync();
        await PostAwardAsync(admin, [("first.png", Png)],
            ("Form.Title", "Award with changing pictures"), ("Form.PublishedOn", "2031-03-01"), ("Form.Excerpt", "Text."));
        var awardId = await AwardIdAsync("Award with changing pictures");
        var first = ImageIdsIn(Award(await PageAsync("/rockstar-awards"), "Award with changing pictures")).Single();

        await PostAwardAsync(admin, [("second.jpg", Jpeg)],
            ("Form.Id", awardId), ("Form.Title", "Award with changing pictures"), ("Form.PublishedOn", "2031-03-01"), ("Form.Excerpt", "Edited."));
        var both = ImageIdsIn(Award(await PageAsync("/rockstar-awards"), "Award with changing pictures"));

        Assert.Equal(2, both.Count);
        Assert.Equal(first, both[0]);

        await PostAwardAsync(admin, [],
            ("Form.Id", awardId), ("Form.Title", "Award with changing pictures"), ("Form.PublishedOn", "2031-03-01"), ("Form.Excerpt", "Edited."),
            ("Form.RemoveImageIds", first.ToString(CultureInfo.InvariantCulture)));

        Assert.Equal([both[1]], ImageIdsIn(Award(await PageAsync("/rockstar-awards"), "Award with changing pictures")));
        Assert.False(await ImageExistsAsync(first));

        await admin.PostFormAsync("/master/rockstar-awards", $"/master/rockstar-awards/{awardId}/delete");

        Assert.False(await ImageExistsAsync(both[1]));
    }

    [Fact]
    public async Task An_award_takes_at_most_ten_pictures()
    {
        using var admin = await AdminAsync();

        using var response = await PostAwardAsync(admin, Enumerable.Range(1, 11).Select(index => ($"photo{index}.png", Png)).ToArray(),
            ("Form.Title", "Award with too many pictures"), ("Form.PublishedOn", "2031-05-01"), ("Form.Excerpt", "Text."));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("up to 10 pictures", await response.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.DoesNotContain("Award with too many pictures", await PageAsync("/rockstar-awards"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("not-a-photo.png", "This file isn&#x27;t a real picture")]
    [InlineData("drawing.svg", "Choose a JPG, PNG or WebP picture.")]
    public async Task Files_that_are_not_photos_are_refused_and_nothing_is_saved(string fileName, string problem)
    {
        using var admin = await AdminAsync();
        int picturesBefore;
        using (var scope = factory.Services.CreateScope())
        {
            picturesBefore = await scope.ServiceProvider.GetRequiredService<PortalDbContext>().PortalImages.CountAsync();
        }

        using var response = await PostAwardAsync(admin, [("good.png", Png), (fileName, "<svg onload=alert(1)>"u8.ToArray())],
            ("Form.Title", $"Refused {fileName}"), ("Form.PublishedOn", "2031-04-01"), ("Form.Excerpt", "Text."));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(problem, html, StringComparison.Ordinal);
        Assert.DoesNotContain($"Refused {fileName}", await PageAsync("/rockstar-awards"), StringComparison.Ordinal);

        using var after = factory.Services.CreateScope();
        Assert.Equal(picturesBefore, await after.ServiceProvider.GetRequiredService<PortalDbContext>().PortalImages.CountAsync());
    }

    [Fact]
    public async Task Ceo_picture_opens_in_the_preview()
    {
        using var admin = await AdminAsync();
        await PostAsync(admin, "/master/ceo-desk", ("ceo.png", Png),
            ("Form.Title", "Message to preview"), ("Form.PostedOn", Day(2)), ("Form.Message", "Text."));

        var card = Section(await PageAsync("/"), "ceo-desk");

        Assert.Matches("<button type=\"button\" class=\"photo-button\" data-photo=\"/media/\\d+\"", card);
    }

    [Fact]
    public async Task Master_lists_show_a_small_picture()
    {
        using var admin = await AdminAsync();
        await PostAsync(admin, "/master/ceo-desk", ("thumb.png", Png),
            ("Form.Title", "Message for the list"), ("Form.PostedOn", Day(-1)), ("Form.Message", "Text."));

        var html = await admin.GetStringAsync(new Uri("/master/ceo-desk", UriKind.Relative));

        Assert.Matches("class=\"master-thumb\" src=\"/media/\\d+\"", html);
    }

    [Fact]
    public async Task Unknown_picture_is_not_found()
    {
        using var visitor = factory.CreateBrowser();

        using var response = await visitor.GetAsync(new Uri("/media/999999", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<HttpClient> AdminAsync()
    {
        var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        return admin;
    }

    private static async Task<HttpResponseMessage> PostAsync(
        HttpClient client, string section, (string Name, byte[] Content)? image, params (string Name, string Value)[] fields)
    {
        var token = await client.GetAntiforgeryTokenAsync(section);

        using var form = new MultipartFormDataContent { { new StringContent(token), "__RequestVerificationToken" } };

        foreach (var (name, value) in fields)
        {
            form.Add(new StringContent(value), name);
        }

        if (image is { } file)
        {
            var content = new ByteArrayContent(file.Content);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "Form.Image", file.Name);
        }

        return await client.PostAsync(new Uri(section, UriKind.Relative), form);
    }

    private static async Task<HttpResponseMessage> PostAwardAsync(
        HttpClient client, (string Name, byte[] Content)[] images, params (string Name, string Value)[] fields)
    {
        var token = await client.GetAntiforgeryTokenAsync("/master/rockstar-awards");

        using var form = new MultipartFormDataContent { { new StringContent(token), "__RequestVerificationToken" } };

        foreach (var (name, value) in fields)
        {
            form.Add(new StringContent(value), name);
        }

        foreach (var (name, content) in images)
        {
            var file = new ByteArrayContent(content);
            file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(file, "Form.Images", name);
        }

        return await client.PostAsync(new Uri("/master/rockstar-awards", UriKind.Relative), form);
    }

    private async Task<string> PageAsync(string path)
    {
        using var visitor = factory.CreateBrowser();

        return await visitor.GetStringAsync(new Uri(path, UriKind.Relative));
    }

    private async Task<string> AwardIdAsync(string title)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();

        var id = await context.RockstarAwards.Where(award => award.Title == title).Select(award => award.Id).SingleAsync();

        return id.ToString(CultureInfo.InvariantCulture);
    }

    private async Task<bool> ImageExistsAsync(int id)
    {
        using var scope = factory.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<PortalDbContext>().PortalImages.AnyAsync(image => image.Id == id);
    }

    private static string Day(int offset) =>
        DateTime.Today.AddDays(offset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Section(string html, string id)
    {
        var start = html.IndexOf($"id=\"{id}\"", StringComparison.Ordinal);

        return html[start..html.IndexOf("</section>", start, StringComparison.Ordinal)];
    }

    // The award card whose title is given, from its opening tag to its end.
    private static string Award(string html, string title)
    {
        var titleAt = html.IndexOf($">{title}</h3>", StringComparison.Ordinal);
        var start = html.LastIndexOf("<article class=\"award\"", titleAt, StringComparison.Ordinal);

        return html[start..html.IndexOf("</article>", titleAt, StringComparison.Ordinal)];
    }

    private static int ImageIdIn(string html) =>
        int.Parse(MediaLink().Match(html).Groups[1].Value, CultureInfo.InvariantCulture);

    // Each picture once, in page order.
    private static List<int> ImageIdsIn(string html) =>
        MediaLink().Matches(html).Select(match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)).Distinct().ToList();

    [GeneratedRegex("/media/(\\d+)")]
    private static partial Regex MediaLink();

    [GeneratedRegex("data-photo=\"/media/\\d+\"")]
    private static partial Regex PreviewPhoto();
}
