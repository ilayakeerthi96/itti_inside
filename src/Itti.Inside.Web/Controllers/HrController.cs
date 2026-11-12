using Itti.Inside.Application.Abstractions;
using Itti.Inside.Domain.Content;
using Itti.Inside.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Itti.Inside.Web.Controllers;

/// <summary>The HR pages in the header menu. The admin keeps them up to date on the master page.</summary>
[AllowAnonymous]
public sealed class HrController(IPortalContentRepository content, TimeProvider clock) : Controller
{
    [HttpGet("hr/corporate-medical-insurance")]
    public async Task<IActionResult> MedicalInsurance(CancellationToken cancellationToken) =>
        View(new MedicalInsurancePageViewModel(
            await content.GetMedicalInsuranceAsync(cancellationToken),
            await content.GetDocumentInfoAsync(PortalDocumentKeys.MedicalInsurancePolicy, cancellationToken)));

    /// <summary>One year's holidays: the year asked for, this year, or else the latest year there is.</summary>
    [HttpGet("hr/holidays")]
    public async Task<IActionResult> Holidays(int? year, CancellationToken cancellationToken)
    {
        var holidays = await content.GetHolidaysAsync(cancellationToken);
        var today = Today;
        var years = holidays.Select(holiday => holiday.Date.Year).Distinct().Order().ToList();
        var shown = year is { } asked && years.Contains(asked) ? asked
            : years.Contains(today.Year) ? today.Year
            : years.Count > 0 ? years[^1] : today.Year;

        return View(new HolidaysPageViewModel(
            shown,
            years,
            holidays.Where(holiday => holiday.Date.Year == shown).ToList(),
            await content.GetHolidayNotesAsync(cancellationToken),
            await content.GetDocumentInfoAsync(PortalDocumentKeys.HolidayList, cancellationToken),
            today));
    }

    [HttpGet("hr/organization-training")]
    public async Task<IActionResult> Training(CancellationToken cancellationToken)
    {
        var sessions = await content.GetTrainingSessionsAsync(cancellationToken);
        var today = Today;

        return View(new TrainingPageViewModel(
            sessions.Where(session => session.Date >= today).ToList(),
            sessions.Where(session => session.Date < today).Reverse().ToList()));
    }

    private DateOnly Today => DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
}

/// <summary>Downloads of the files the admin uploads, such as the holiday list.</summary>
[AllowAnonymous]
public sealed class DocumentsController(IPortalContentRepository content) : Controller
{
    [HttpGet("documents/{key}")]
    public async Task<IActionResult> Download(string key, CancellationToken cancellationToken)
    {
        if (!PortalDocumentKeys.IsKnown(key) || await content.GetDocumentAsync(key, cancellationToken) is not { } document)
        {
            return NotFound();
        }

        // Sent as an attachment with the stored type, so the browser saves it rather than running it.
        return File(document.Content, document.ContentType, document.FileName);
    }
}
