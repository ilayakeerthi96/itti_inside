using System.Globalization;
using System.Security.Claims;
using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Web.Models.Master;
using Itti.Inside.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers.Master;

[Route("master/ceo-desk")]
public sealed class CeoDeskMasterController(IPortalContentEditor editor, ILogger<CeoDeskMasterController> logger)
    : MasterSectionController<CeoDeskPost, CeoDeskPostForm>(editor, logger)
{
    protected override string SectionView => "CeoDesk";

    protected override IEnumerable<CeoDeskPost> Arrange(IEnumerable<CeoDeskPost> items) =>
        items.OrderByDescending(post => post.PostedOn).ThenByDescending(post => post.Id);
}

[Route("master/training")]
public sealed class TrainingMasterController(IPortalContentEditor editor, ILogger<TrainingMasterController> logger)
    : MasterSectionController<TrainingSession, TrainingSessionForm>(editor, logger)
{
    protected override string SectionView => "Training";

    protected override IEnumerable<TrainingSession> Arrange(IEnumerable<TrainingSession> items) =>
        items.OrderByDescending(session => session.Date).ThenByDescending(session => session.Id);
}

/// <summary>The holiday list: one holiday at a time, or the whole year from HR's Word file.</summary>
[Route("master/holidays")]
public sealed class HolidaysMasterController(
    IPortalContentEditor editor,
    IPortalContentRepository content,
    IHolidayListReader reader,
    TimeProvider clock,
    ILogger<HolidaysMasterController> logger)
    : MasterSectionController<Holiday, HolidayForm>(editor, logger)
{
    private static readonly string[] AllowedFiles = [".docx", ".pdf", ".xlsx"];

    protected override string SectionView => "Holidays";

    protected override PortalTextKey? TextKey => PortalTextKey.HolidayNotes;

    protected override IEnumerable<Holiday> Arrange(IEnumerable<Holiday> items) =>
        items.OrderByDescending(holiday => holiday.Date.Year).ThenBy(holiday => holiday.Date);

    protected override async Task AddViewDataAsync(CancellationToken cancellationToken) =>
        ViewData[nameof(PortalDocumentInfo)] = await content.GetDocumentInfoAsync(PortalDocumentKeys.HolidayList, cancellationToken);

    [HttpPost("upload")]
    [RequestSizeLimit(DocumentUpload.RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = DocumentUpload.RequestLimit)]
    public async Task<IActionResult> Upload(IFormFile? file, bool updateList, CancellationToken cancellationToken)
    {
        var (document, problem) = await DocumentUpload.ReadAsync(
            file, PortalDocumentKeys.HolidayList, AllowedFiles, clock.GetUtcNow(), cancellationToken);

        if (document is null)
        {
            TempData[MasterNotice.Key] = problem;
            return RedirectToAction(nameof(Index));
        }

        await Editor.SaveDocumentAsync(document, cancellationToken);
        Logger.DocumentUploaded(CurrentUserId, document.FileName, document.Content.Length, document.Key);

        var notice = "Uploaded. Employees can download it from the Holidays page.";

        if (updateList && DocumentUpload.IsWordFile(document))
        {
            using var stream = new MemoryStream(document.Content, writable: false);
            var holidays = reader.Read(stream);

            if (holidays.Count > 0)
            {
                await Editor.ReplaceHolidaysAsync(holidays, cancellationToken);
                Logger.HolidaysImported(CurrentUserId, holidays.Count);

                var years = string.Join(" and ", holidays.Select(holiday => holiday.Date.Year).Distinct().Order()
                    .Select(year => year.ToString(CultureInfo.InvariantCulture)));
                notice = $"Uploaded. The Holidays page now lists the {holidays.Count} holidays for {years} from the file.";
            }
            else
            {
                notice = "Uploaded, but the file has no table with a date column, so the holiday list didn't change.";
            }
        }

        TempData[MasterNotice.Key] = notice;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("document/delete")]
    public async Task<IActionResult> DeleteDocument(CancellationToken cancellationToken)
    {
        await Editor.DeleteDocumentAsync(PortalDocumentKeys.HolidayList, cancellationToken);
        Logger.DocumentDeleted(CurrentUserId, PortalDocumentKeys.HolidayList);

        TempData[MasterNotice.Key] = MasterNotice.FileDeleted;
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>The company's one medical insurance record, edited as a whole, plus the policy document.</summary>
[Authorize(Policy = PortalPolicies.ManageContent)]
[Route("master/medical-insurance")]
public sealed class MedicalInsuranceMasterController(
    IPortalContentEditor editor,
    IPortalContentRepository content,
    TimeProvider clock,
    ILogger<MedicalInsuranceMasterController> logger) : Controller
{
    private static readonly string[] AllowedFiles = [".pdf", ".docx"];

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var form = new MedicalInsuranceForm();

        if (await content.GetMedicalInsuranceAsync(cancellationToken) is { } policy)
        {
            form.Load(policy);
        }

        return await ShowAsync(form, cancellationToken);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(MedicalInsuranceForm form, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(form);

        if (!ModelState.IsValid)
        {
            return await ShowAsync(form, cancellationToken);
        }

        // There is only one record: an existing one is always the one that is changed.
        var existing = await content.GetMedicalInsuranceAsync(cancellationToken);
        var policy = form.ToItem() with { Id = existing?.Id ?? 0 };

        await editor.SaveAsync(policy, cancellationToken);
        logger.ItemSaved(CurrentUserId, "MedicalInsurance", policy.Id);

        TempData[MasterNotice.Key] = MasterNotice.Saved;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(DocumentUpload.RequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = DocumentUpload.RequestLimit)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        var (document, problem) = await DocumentUpload.ReadAsync(
            file, PortalDocumentKeys.MedicalInsurancePolicy, AllowedFiles, clock.GetUtcNow(), cancellationToken);

        if (document is not null)
        {
            await editor.SaveDocumentAsync(document, cancellationToken);
            logger.DocumentUploaded(CurrentUserId, document.FileName, document.Content.Length, document.Key);
        }

        TempData[MasterNotice.Key] = problem ?? "Uploaded. Employees can download it from the Corporate Medical Insurance page.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("document/delete")]
    public async Task<IActionResult> DeleteDocument(CancellationToken cancellationToken)
    {
        await editor.DeleteDocumentAsync(PortalDocumentKeys.MedicalInsurancePolicy, cancellationToken);
        logger.DocumentDeleted(CurrentUserId, PortalDocumentKeys.MedicalInsurancePolicy);

        TempData[MasterNotice.Key] = MasterNotice.FileDeleted;
        return RedirectToAction(nameof(Index));
    }

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<IActionResult> ShowAsync(MedicalInsuranceForm form, CancellationToken cancellationToken)
    {
        ViewData[nameof(PortalDocumentInfo)] =
            await content.GetDocumentInfoAsync(PortalDocumentKeys.MedicalInsurancePolicy, cancellationToken);

        return View("~/Views/Master/MedicalInsurance.cshtml", form);
    }
}
