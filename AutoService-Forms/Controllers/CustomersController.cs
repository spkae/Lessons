
using AutoService.Data;
using AutoService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Controllers;

public class CustomersController : Controller
{
    private readonly AppDbContext _context;
    public CustomersController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var items = await _context.Customers.Include(c => c.Vehicles).ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer c)
    {
        if (ModelState.IsValid)
        {
            _context.Customers.Add(c);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(c);
    }

}
