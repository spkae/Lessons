
using AutoService.Data;
using AutoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers;

public class JobOrdersController : Controller
{
    private readonly AppDbContext _context;
    public JobOrdersController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var items = await _context.JobOrders.Include(j => j.Vehicle).ThenInclude(v => v.Customer).ToListAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var job = await _context.JobOrders
            .Include(j => j.Vehicle).ThenInclude(v => v.Customer)
            .Include(j => j.Parts).ThenInclude(jp => jp.Part)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();
        ViewBag.PartsList = await _context.Parts.OrderBy(p => p.Name).ToListAsync();
        return View(job);
    }

    [HttpPost]
    public async Task<IActionResult> AddPart(int jobOrderId, int partId, int quantity, decimal? unitPrice)
    {
        var job = await _context.JobOrders.Include(j => j.Parts).FirstOrDefaultAsync(j => j.Id == jobOrderId);
        var part = await _context.Parts.FindAsync(partId);
        if (job == null || part == null) return NotFound();

        var price = unitPrice ?? part.UnitPrice ?? 0m;
        var existing = job.Parts.FirstOrDefault(x => x.PartId == partId && x.UnitPrice == price);
        if (existing != null) existing.Quantity += quantity;
        else job.Parts.Add(new JobPart { PartId = partId, Quantity = quantity, UnitPrice = price });

        part.StockQty -= quantity;
        if (part.StockQty < 0) part.StockQty = 0;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = jobOrderId });
    }

    [HttpPost]
    public async Task<IActionResult> RemovePart(int id)
    {
        var jp = await _context.JobParts.Include(x => x.Part).FirstOrDefaultAsync(x => x.Id == id);
        if (jp == null) return NotFound();
        var jobId = jp.JobOrderId;

        if (jp.Part != null) jp.Part.StockQty += jp.Quantity;

        _context.JobParts.Remove(jp);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = jobId });
    }
}
