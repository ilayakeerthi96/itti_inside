using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Infrastructure.Content;
using Itti.Inside.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itti.Inside.Tests;

public sealed class WordHolidayListReaderTests
{
    private static readonly WordHolidayListReader Reader = new();

    [Fact]
    public void Reads_the_first_table_of_HRs_holiday_list()
    {
        using var file = new MemoryStream(WordFiles.HolidayList2026());

        var holidays = Reader.Read(file);

        Assert.Equal(
            [
                new Holiday(0, new DateOnly(2026, 1, 1), "New Year", false),
                new Holiday(0, new DateOnly(2026, 3, 4), "Holi", true),
                new Holiday(0, new DateOnly(2026, 3, 19), "Ugadi / Gudi Padwa", false),
                new Holiday(0, new DateOnly(2026, 5, 1), "May Day (International Worker’s Day)", false),
                new Holiday(0, new DateOnly(2026, 11, 1), "Kannada Rajyotsava/ Naraka Chaturdashi", false),
            ],
            holidays);
    }

    [Fact]
    public void A_table_without_a_date_column_gives_nothing()
    {
        using var file = new MemoryStream(WordFiles.WithTables([["Name", "Team"], ["Someone", "IMS"]]));

        Assert.Empty(Reader.Read(file));
    }

    [Fact]
    public void A_file_that_is_not_a_Word_file_gives_nothing()
    {
        using var file = new MemoryStream("not a word file"u8.ToArray());

        Assert.Empty(Reader.Read(file));
    }
}

public sealed class HrPagesTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task Hr_menu_has_only_the_three_hr_pages()
    {
        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));
        // Razor writes characters such as the dash as HTML codes, so compare the text a browser shows.
        var header = WebUtility.HtmlDecode(html[..html.IndexOf("</header>", StringComparison.Ordinal)]);

        Assert.Contains("Corporate Medical Insurance", header, StringComparison.Ordinal);
        Assert.Contains("HOLIDAYS – 2026", header, StringComparison.Ordinal);
        Assert.Contains("Organization Training", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Useful Links", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Travel desk", header, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Medical_insurance_page_shows_the_policy()
    {
        using var client = factory.CreateBrowser();

        var html = await client.GetStringAsync(new Uri("/hr/corporate-medical-insurance", UriKind.Relative));

        Assert.Contains("Sample Insurance Company Ltd.", html, StringComparison.Ordinal);
        Assert.Contains("Key benefits", html, StringComparison.Ordinal);
        Assert.Contains("How to make a claim", html, StringComparison.Ordinal);
        Assert.Contains("hr.helpdesk@example.com", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Training_page_splits_upcoming_and_past()
    {
        await SaveAsync(new TrainingSession(0, "Future session", "Technical", Today.AddDays(10), "", "Online", "", "", "", "", "https://example.com/register"));
        await SaveAsync(new TrainingSession(0, "Finished session", "Leadership", Today.AddDays(-10), "", "Classroom", "", "", "", "", ""));

        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/hr/organization-training", UriKind.Relative));
        var past = html.IndexOf("Past trainings", StringComparison.Ordinal);

        Assert.InRange(html.IndexOf("Future session", StringComparison.Ordinal), 0, past);
        Assert.True(html.IndexOf("Finished session", StringComparison.Ordinal) > past);
        Assert.Contains(PageLinks.All(html), link => link.Text == "Register" && link.OpensInNewTab);
    }

    [Fact]
    public async Task Holidays_page_lists_the_year_with_the_next_holiday_and_notes()
    {
        var year = Today.Year;
        await SaveAsync(new Holiday(0, Today.AddDays(1), "Tomorrow's holiday", false));
        await SaveAsync(new Holiday(0, new DateOnly(year, 1, 1), "First of the year", true));

        using var client = factory.CreateBrowser();
        var html = await client.GetStringAsync(new Uri("/hr/holidays", UriKind.Relative));

        Assert.Contains($"List of holidays – {Today.AddDays(1).Year}", html, StringComparison.Ordinal);
        Assert.Contains("Next holiday: <strong>Tomorrow&#x27;s holiday</strong>", html, StringComparison.Ordinal);
        Assert.Contains("Important notes", html, StringComparison.Ordinal);
        Assert.Contains("Karnataka State Government", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unknown_document_is_not_found()
    {
        using var client = factory.CreateBrowser();

        using var response = await client.GetAsync(new Uri("/documents/web.config", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private async Task SaveAsync<T>(T item)
        where T : class, IContentItem
    {
        using var scope = factory.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IPortalContentEditor>().SaveAsync(item, CancellationToken.None);
    }
}

/// <summary>The master page HR sections. Own database, because uploads replace the holiday list.</summary>
public sealed class HrMasterTests(PortalWebFactory factory) : IClassFixture<PortalWebFactory>
{
    [Fact]
    public async Task Uploading_HRs_Word_file_fills_the_holiday_list_and_offers_the_file()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        using var upload = await UploadAsync(admin, "/master/holidays", "/master/holidays/upload", "ITTI_HOLIDAY LIST - 2026_1.0.docx", WordFiles.HolidayList2026(), updateList: true);
        var notice = await admin.GetStringAsync(new Uri("/master/holidays", UriKind.Relative));

        using var visitor = factory.CreateBrowser();
        var page = await visitor.GetStringAsync(new Uri("/hr/holidays?year=2026", UriKind.Relative));
        using var download = await visitor.GetAsync(new Uri("/documents/holiday-list", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, upload.StatusCode);
        Assert.Contains("now lists the 5 holidays for 2026", notice, StringComparison.Ordinal);
        Assert.Contains("Ugadi / Gudi Padwa", page, StringComparison.Ordinal);
        Assert.DoesNotContain("From The Second Table", page, StringComparison.Ordinal);
        Assert.Contains("Download the holiday list", page, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal("attachment", download.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(WordFiles.HolidayList2026().Length, (await download.Content.ReadAsByteArrayAsync()).Length);
    }

    [Fact]
    public async Task Uploading_again_replaces_that_years_holidays()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        await UploadAsync(admin, "/master/holidays", "/master/holidays/upload", "first.docx", WordFiles.HolidayList2026(), updateList: true);
        await UploadAsync(admin, "/master/holidays", "/master/holidays/upload", "second.docx",
            WordFiles.WithTables([["Holiday", "Date"], ["Only holiday left", "15-08-2026"]]), updateList: true);

        int count;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PortalDbContext>();
            count = await context.Holidays.CountAsync(holiday => holiday.Date >= new DateOnly(2026, 1, 1) && holiday.Date <= new DateOnly(2026, 12, 31));
        }

        Assert.Equal(1, count);
    }

    [Theory]
    [InlineData("holidays.exe", "MZ")]
    [InlineData("holidays.pdf", "PK not a pdf")]
    public async Task Files_that_are_not_documents_are_refused(string fileName, string content)
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        await UploadAsync(admin, "/master/medical-insurance", "/master/medical-insurance/upload", fileName, System.Text.Encoding.ASCII.GetBytes(content), updateList: false);
        var page = await admin.GetStringAsync(new Uri("/master/medical-insurance", UriKind.Relative));

        Assert.Contains("Nothing was uploaded", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Signed_out_visitor_cannot_upload()
    {
        using var visitor = factory.CreateBrowser();
        var token = await visitor.GetAntiforgeryTokenAsync("/account/login");

        using var form = new MultipartFormDataContent { { new StringContent(token), "__RequestVerificationToken" } };
        using var response = await visitor.PostAsync(new Uri("/master/holidays/upload", UriKind.Relative), form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/account/login", response.Headers.Location?.PathAndQuery, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saving_the_medical_insurance_updates_its_page()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        using var response = await admin.PostFormAsync(
            "/master/medical-insurance",
            "/master/medical-insurance",
            new("Insurer", "Changed Insurance Co."),
            new("ValidFrom", "2026-04-01"),
            new("ValidTo", "2027-03-31"),
            new("SumInsured", "₹5,00,000 per family"),
            new("Coverage", "Employee and family"),
            new("Benefits", "First benefit\nSecond benefit"),
            new("NetworkHospitalsLink", "https://hospitals.example.com"));

        using var visitor = factory.CreateBrowser();
        var page = await visitor.GetStringAsync(new Uri("/hr/corporate-medical-insurance", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Changed Insurance Co.", page, StringComparison.Ordinal);
        Assert.Contains("<li>Second benefit</li>", page, StringComparison.Ordinal);
        Assert.Contains(PageLinks.All(page), link => link.Href == "https://hospitals.example.com/" || link.Href == "https://hospitals.example.com");
    }

    [Fact]
    public async Task Medical_insurance_rejects_an_end_date_before_the_start()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();

        using var response = await admin.PostFormAsync(
            "/master/medical-insurance",
            "/master/medical-insurance",
            new("Insurer", "Backwards Insurance"),
            new("ValidFrom", "2027-04-01"),
            new("ValidTo", "2026-03-31"),
            new("SumInsured", "Any"),
            new("Coverage", "Any"),
            new("NetworkHospitalsLink", "javascript:alert(1)"));

        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The end date must be on or after the start date.", html, StringComparison.Ordinal);
        Assert.Contains("Enter a full web address", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Ceo_desk_page_shows_the_newest_message_in_full_and_the_earlier_ones()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();
        var longMessage = string.Join(' ', Enumerable.Repeat("Growth and customers.", 40));

        await admin.PostFormAsync("/master/ceo-desk", "/master/ceo-desk",
            new("Form.Title", "Older message"), new("Form.PostedOn", Day(-30)), new("Form.Message", "An earlier word."));
        await admin.PostFormAsync("/master/ceo-desk", "/master/ceo-desk",
            new("Form.Title", "Newest message"), new("Form.PostedOn", Day(0)), new("Form.Message", longMessage));

        using var visitor = factory.CreateBrowser();
        var home = WebUtility.HtmlDecode(await visitor.GetStringAsync(new Uri("/", UriKind.Relative)));
        var page = await visitor.GetStringAsync(new Uri("/ceo-desk", UriKind.Relative));

        Assert.Contains("Newest message", home, StringComparison.Ordinal);
        Assert.DoesNotContain(longMessage, home, StringComparison.Ordinal);
        Assert.Contains("…", home, StringComparison.Ordinal);
        Assert.Contains(longMessage, page, StringComparison.Ordinal);
        Assert.Contains("Earlier messages", page, StringComparison.Ordinal);
        Assert.Contains("An earlier word.", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Ceo_message_longer_than_4000_characters_is_kept_in_full()
    {
        using var admin = factory.CreateBrowser();
        await admin.SignInAsAdminAsync();
        var letter = string.Concat(Enumerable.Repeat("A long letter from the CEO. ", 300)).Trim();

        using var response = await admin.PostFormAsync("/master/ceo-desk", "/master/ceo-desk",
            new("Form.Title", "A very long message"), new("Form.PostedOn", Day(1)), new("Form.Message", letter));

        using var visitor = factory.CreateBrowser();
        var page = await visitor.GetStringAsync(new Uri("/ceo-desk", UriKind.Relative));

        Assert.True(letter.Length > 8000);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(letter, page, StringComparison.Ordinal);
    }

    private static string Day(int offset) =>
        DateTime.Today.AddDays(offset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static async Task<HttpResponseMessage> UploadAsync(
        HttpClient client, string pagePath, string actionPath, string fileName, byte[] content, bool updateList)
    {
        var token = await client.GetAntiforgeryTokenAsync(pagePath);
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var form = new MultipartFormDataContent
        {
            { new StringContent(token), "__RequestVerificationToken" },
            { new StringContent(updateList ? "true" : "false"), "updateList" },
            { file, "file", fileName },
        };

        return await client.PostAsync(new Uri(actionPath, UriKind.Relative), form);
    }
}
