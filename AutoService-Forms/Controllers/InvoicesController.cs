
using AutoService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoService.Controllers;

public class InvoicesController : Controller
{
    private readonly InvoiceService _svc;
    public InvoicesController(InvoiceService svc) => _svc = svc;

    [HttpGet("/Invoices/Job/{id:int}")]
    public async Task<IActionResult> Job(int id)
    {
        var pdf = await _svc.RenderJobInvoicePdf(id);
        return File(pdf, "application/pdf", $"invoice-job-{id}.pdf");
    }
}
