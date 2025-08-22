
using AutoService.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoService.Services;

public class InvoiceService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public InvoiceService(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> RenderJobInvoicePdf(int jobId)
    {
        var job = await _db.JobOrders
            .Include(j => j.Vehicle).ThenInclude(v => v.Customer)
            .Include(j => j.Parts).ThenInclude(p => p.Part)
            .FirstOrDefaultAsync(j => j.Id == jobId);

        if (job == null) throw new InvalidOperationException("Job not found");

        var rate = _cfg.GetSection("Taxes").GetValue<decimal?>("HstRate") ?? 0m;
        var subtotal = job.Subtotal;
        var tax = Math.Round(subtotal * rate, 2);
        var total = subtotal + tax;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("AutoService — Work Order / Invoice").SemiBold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    col.Item().Text($"Job: #{job.Id} — {job.Title}");
                    col.Item().Text($"Vehicle: {job.Vehicle?.Year} {job.Vehicle?.Make} {job.Vehicle?.Model} | VIN: {job.Vehicle?.Vin} | Plate: {job.Vehicle?.Plate}");
                    col.Item().Text($"Customer: {job.Vehicle?.Customer?.FullName}  Phone: {job.Vehicle?.Customer?.Phone}");
                    col.Item().LineHorizontal(1);
                    col.Item().Text("Parts").SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(5); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(2); });
                        t.Header(h => { h.Cell().Text("Part"); h.Cell().Text("SKU"); h.Cell().Text("Qty"); h.Cell().Text("Unit Price"); h.Cell().Text("Line Total"); });
                        foreach (var jp in job.Parts)
                        {
                            t.Cell().Text(jp.Part?.Name);
                            t.Cell().Text(jp.Part?.Sku);
                            t.Cell().Text(jp.Quantity.ToString());
                            t.Cell().Text($"{jp.UnitPrice:0.00}");
                            t.Cell().Text($"{(jp.UnitPrice * jp.Quantity):0.00}");
                        }
                    });
                    col.Item().Text("");
                    col.Item().Text($"Labor: {job.LaborHours:0.00} h × {job.LaborRate:0.00} = {job.LaborTotal:0.00}");
                    col.Item().AlignRight().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(1); });
                        t.Cell().Text("Subtotal:").AlignRight();
                        t.Cell().Text($"{subtotal:0.00}").AlignRight();
                        t.Cell().Text($"HST ({rate:P0}):").AlignRight();
                        t.Cell().Text($"{tax:0.00}").AlignRight();
                        t.Cell().Text("TOTAL:").SemiBold().AlignRight();
                        t.Cell().Text($"{total:0.00}").SemiBold().AlignRight();
                    });
                    if (!string.IsNullOrWhiteSpace(job.Notes))
                    {
                        col.Item().Text("Notes:").SemiBold();
                        col.Item().Text(job.Notes);
                    }
                });
                page.Footer().AlignCenter().Text("Thank you for your business!");
            });
        });

        return doc.GeneratePdf();
    }
}
