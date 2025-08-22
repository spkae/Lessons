
using AutoService.Data;
using AutoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers;

public class VehiclesController : Controller
{
    private readonly AppDbContext _context;
    public VehiclesController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var items = await _context.Vehicles.Include(v => v.Customer).ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .Include(v => v.Jobs).ThenInclude(j => j.Parts)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null) return NotFound();
        return View(vehicle);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Customers = await _context.Customers
            .OrderBy(c => c.FullName)
            .ToListAsync();
        return View(new Vehicle { Year = DateTime.Now.Year });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle v)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync();
            return View(v);
        }

        // необязательно: простая защита от дублей VIN/Plate
        if (!string.IsNullOrWhiteSpace(v.Vin) &&
            await _context.Vehicles.AnyAsync(x => x.Vin == v.Vin))
            ModelState.AddModelError(nameof(Vehicle.Vin), "ТС с таким VIN уже есть.");

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync();
            return View(v);
        }

        _context.Vehicles.Add(v);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = v.Id });
    }
}
