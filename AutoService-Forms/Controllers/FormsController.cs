
using System.Text.Json;
using AutoService.Data;
using AutoService.Models;
using AutoService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers;

public class FormsController : Controller
{
    private readonly AppDbContext _db;
    private readonly OceanexPdfService _oceanexPdf;
    private readonly WorkSheetPdfService _workPdf;

    public FormsController(AppDbContext db, OceanexPdfService oceanexPdf, WorkSheetPdfService workPdf)
    {
        _db = db; _oceanexPdf = oceanexPdf; _workPdf = workPdf;
    }

    [HttpGet]
    public async Task<IActionResult> New(int vehicleId, int? jobId)
    {
        ViewBag.Vehicle = await _db.Vehicles.Include(v => v.Customer).FirstAsync(v => v.Id == vehicleId);
        ViewBag.JobId = jobId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Oceanex(int vehicleId, int? jobId)
    {
        var v = await _db.Vehicles.Include(x => x.Customer).FirstAsync(x => x.Id == vehicleId);
        ViewBag.Vehicle = v; ViewBag.JobId = jobId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> OceanexSubmit(int vehicleId, int? jobId, string title, [FromForm] Dictionary<string,string> form)
    {
        var entry = new FormEntry
        {
            VehicleId = vehicleId, JobOrderId = jobId,
            Company = "Oceanex", TemplateCode = "Oceanex_PM_MVIC_v1",
            Type = FormType.Inspection, Title = string.IsNullOrWhiteSpace(title) ? "PM/MVIC" : title,
            DataJson = JsonSerializer.Serialize(form)
        };
        _db.FormEntries.Add(entry);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = entry.Id });
    }

    [HttpGet]
    public async Task<IActionResult> WorkSheet(int vehicleId, int jobId)
    {
        ViewBag.Vehicle = await _db.Vehicles.Include(x => x.Customer).FirstAsync(x => x.Id == vehicleId);
        ViewBag.Job = await _db.JobOrders.Include(j=>j.Vehicle).ThenInclude(v=>v.Customer).FirstAsync(j=>j.Id==jobId);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> WorkSheetSubmit(int vehicleId, int jobId, string title, [FromForm] Dictionary<string,string> form)
    {
        var entry = new FormEntry
        {
            VehicleId = vehicleId, JobOrderId = jobId,
            Company = "Generic", TemplateCode = "Blue_Work_v1",
            Type = FormType.WorkSheet, Title = string.IsNullOrWhiteSpace(title) ? "Work Sheet" : title,
            DataJson = JsonSerializer.Serialize(form)
        };
        _db.FormEntries.Add(entry);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = entry.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var e = await _db.FormEntries.Include(f=>f.Vehicle).ThenInclude(v=>v.Customer)
                                     .Include(f=>f.JobOrder).FirstAsync(f=>f.Id==id);
        return View(e);
    }

    [HttpGet("/Forms/Pdf/{id:int}")]
    public async Task<IActionResult> Pdf(int id)
    {
        var e = await _db.FormEntries.Include(f=>f.Vehicle).ThenInclude(v=>v.Customer)
                                     .Include(f=>f.JobOrder).FirstAsync(f=>f.Id==id);
        byte[] pdf = e.TemplateCode switch
        {
            "Oceanex_PM_MVIC_v1" => await _oceanexPdf.Render(e),
            "Blue_Work_v1"       => await _workPdf.Render(e),
            _ => Array.Empty<byte>()
        };
        if (pdf.Length == 0) return NotFound();
        return File(pdf, "application/pdf", $"{e.TemplateCode}-{e.Id}.pdf");
    }
}
