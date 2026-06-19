using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationReport.Data;
using WebApplicationReport.Models.Domain;
using WebApplicationReport.Models.ViewModels;
using WebApplicationReport.Services;

namespace WebApplicationReport.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RdlcService _rdlcService;

    public ReportController(ApplicationDbContext db,
                            UserManager<ApplicationUser> userManager,
                            RdlcService rdlcService)
    {
        _db = db;
        _userManager = userManager;
        _rdlcService = rdlcService;
    }

    // ── Index ─────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,DataEntry,Editor")]
    public async Task<IActionResult> Index()
    {
        var reports = await _db.Reports
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(reports);
    }

    // ── Create ────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new ReportCreateViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReportCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User)!;
        var report = new Report
        {
            Name = model.Name,
            Description = model.Description,
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now
        };

        for (int i = 0; i < model.Fields.Count; i++)
        {
            var f = model.Fields[i];
            report.Fields.Add(new ReportField
            {
                Label = f.Label,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                Order = i,
                Options = f.Options
            });
        }

        _db.Reports.Add(report);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ── Details / Viewer ─────────────────────────────────────────────────
    [Authorize(Roles = "Admin,DataEntry,Editor")]
    public async Task<IActionResult> DetailsEntry(int id)
    {
        var report = await _db.Reports
            .Include(r => r.Fields.OrderBy(f => f.Order))
            .Include(r => r.Entries.Where(e => !e.IsDeleted))
                .ThenInclude(e => e.Values)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null) return NotFound();
        return View(report);
    }

    // ── Add Entry ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,DataEntry")]
    public async Task<IActionResult> AddEntry(int id)
    {
        var report = await _db.Reports
            .Include(r => r.Fields.OrderBy(f => f.Order))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null) return NotFound();

        var model = new ReportEntryViewModel
        {
            ReportId = report.Id,
            ReportName = report.Name,
            Fields = report.Fields.Select(f => new ReportEntryFieldViewModel
            {
                FieldId = f.Id,
                Label = f.Label,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                Options = f.Options?.Split(',', StringSplitOptions.TrimEntries).ToList() ?? []
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,DataEntry")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(int id, ReportEntryViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        var entry = new ReportEntry
        {
            ReportId = id,
            SubmittedByUserId = user!.Id,
            SubmittedByEmail = user.Email!,
            SubmittedAt = DateTime.Now,
            Values = model.Fields.Select(f => new ReportEntryValue
            {
                ReportFieldId = f.FieldId,
                Value = f.Value
            }).ToList()
        };

        _db.ReportEntries.Add(entry);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(DetailsEntry), new { id });
    }

    // ── Edit Entry ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> EditEntry(int id)
    {
        var entry = await _db.ReportEntries
            .Include(e => e.Report)
                .ThenInclude(r => r.Fields.OrderBy(f => f.Order))
            .Include(e => e.Values)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry is null) return NotFound();

        var model = new ReportEntryViewModel
        {
            ReportId = entry.ReportId,
            ReportName = entry.Report.Name,
            Fields = entry.Report.Fields.Select(f => new ReportEntryFieldViewModel
            {
                FieldId = f.Id,
                Label = f.Label,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                Options = f.Options?.Split(',', StringSplitOptions.TrimEntries).ToList() ?? [],
                Value = entry.Values.FirstOrDefault(v => v.ReportFieldId == f.Id)?.Value
            }).ToList()
        };

        ViewBag.EntryId = id;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEntry(int id, ReportEntryViewModel model)
    {
        var entry = await _db.ReportEntries
            .Include(e => e.Values)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry is null) return NotFound();

        foreach (var fieldVm in model.Fields)
        {
            var val = entry.Values.FirstOrDefault(v => v.ReportFieldId == fieldVm.FieldId);
            if (val is not null)
                val.Value = fieldVm.Value;
            else
                entry.Values.Add(new ReportEntryValue { ReportFieldId = fieldVm.FieldId, Value = fieldVm.Value });
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(DetailsEntry), new { id = entry.ReportId });
    }

    // ── Delete Entry ──────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var entry = await _db.ReportEntries
            .Include(e => e.Values)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry is null) return NotFound();

        var reportId = entry.ReportId;
        entry.IsDeleted = true;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(DetailsEntry), new { id = reportId });
    }

    // ── RDLC Preview (PDF in iframe) ──────────────────────────────────────
    [Authorize(Roles = "Admin,DataEntry")]
    public async Task<IActionResult> Preview(int id)
    {
        var (report, fields, entries) = await LoadReportData(id);
        if (report is null) return NotFound();

        var pdf = _rdlcService.Render(report, fields, entries, "PDF");
        return File(pdf, "application/pdf");
    }

    // ── Export Excel ──────────────────────────────────────────────────────
    [Authorize(Roles = "Admin,DataEntry")]
    public async Task<IActionResult> ExportExcel(int id)
    {
        var (report, fields, entries) = await LoadReportData(id);
        if (report is null) return NotFound();

        var excel = _rdlcService.Render(report, fields, entries, "EXCELOPENXML");
        var fileName = $"{report.Name}_{DateTime.Now:yyyyMMdd}.xlsx";
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private async Task<(Report? report, List<ReportField> fields, List<ReportEntry> entries)> LoadReportData(int id)
    {
        var report = await _db.Reports
            .Include(r => r.Fields.OrderBy(f => f.Order))
            .Include(r => r.Entries.Where(e => !e.IsDeleted))
                .ThenInclude(e => e.Values)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report is null) return (null, [], []);
        return (report, report.Fields.OrderBy(f => f.Order).ToList(), report.Entries.ToList());
    }
}
