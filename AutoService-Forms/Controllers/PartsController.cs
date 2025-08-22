
using AutoService.Data;
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
}
