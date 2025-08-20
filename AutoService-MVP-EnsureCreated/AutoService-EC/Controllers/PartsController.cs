
using AutoService.Data;
using AutoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers;

public class PartsController : Controller
{
    private readonly AppDbContext _context;
    public PartsController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var items = await _context.Parts.OrderBy(p => p.Name).ToListAsync();
        return View(items);
    }

    [HttpGet] public IActionResult Create() => View(new Part());

    [HttpPost]
    public async Task<IActionResult> Create(Part model)
    {
        if (!ModelState.IsValid) return View(model);
        _context.Parts.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
