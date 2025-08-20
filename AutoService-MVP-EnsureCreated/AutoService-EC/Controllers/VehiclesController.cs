
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
        ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync();
        return View(new Vehicle { Year = DateTime.Now.Year });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Vehicle model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync();
            return View(model);
        }
        _context.Vehicles.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
