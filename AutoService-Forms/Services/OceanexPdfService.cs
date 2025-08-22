
using System.Text.Json;
using AutoService.Data;
using AutoService.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoService.Services;

public class OceanexPdfService
{
    private readonly AppDbContext _db;
    public OceanexPdfService(AppDbContext db){ _db = db; QuestPDF.Settings.License = LicenseType.Community; }

    public async Task<byte[]> Render(FormEntry entry)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string,string>>(entry.DataJson) ?? new();
        var v = await _db.Vehicles.Include(x=>x.Customer).FirstAsync(x=>x.Id==entry.VehicleId);

        var doc = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(25);
                p.Header().Row(r => {
                    r.RelativeItem().Text("OCEANEX").Bold().FontSize(16);
                    r.ConstantItem(200).AlignRight().Text("Preventive Maintenance & MVIC\nVersion 1").FontSize(10);
                });
                p.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                        t.Cell().Text($"Make: {v.Make}");
                        t.Cell().Text($"Model: {v.Model}");
                        t.Cell().Text($"Year: {v.Year}");
                        t.Cell().Text($"Unit#: {data.GetValueOrDefault("unit","")}");
                        t.Cell().Text($"License Plate: {data.GetValueOrDefault("plate", v.Plate ?? "")}");
                        t.Cell().Text($"VIN: {data.GetValueOrDefault("vin", v.Vin ?? "")}");
                        t.Cell().Text($"KM: {data.GetValueOrDefault("km","")}");
                        t.Cell().Text($"Next MCIC: {data.GetValueOrDefault("next_mvic","")}");
                    });

                    col.Item().Text("Checklist (C/NC/R)").SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(3); cd.RelativeColumn(); });
                        t.Header(h => { h.Cell().Text("Description"); h.Cell().Text("State"); });

                        for (int i=0;i<60;i++)
                        {
                            var key = $"chk_{i}";
                            if (!data.ContainsKey(key)) continue;
                            t.Cell().Text($"Item {i+1}");
                            t.Cell().Text(data[key]);
                        }
                    });

                    col.Item().Text("");
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                        t.Cell().Text($"Next MVIC: {data.GetValueOrDefault("next_mvic","")}");
                        t.Cell().Text($"Next Maintenance: {data.GetValueOrDefault("next_mtc","")}");
                        t.Cell().Text($"Mechanic: {data.GetValueOrDefault("mechanic","")}");
                        t.Cell().Text($"Employee#: {data.GetValueOrDefault("employee","")}");
                    });

                    if (data.TryGetValue("notes", out var notes) && !string.IsNullOrWhiteSpace(notes))
                    {
                        col.Item().Text("Notes:").SemiBold();
                        col.Item().Background(Colors.Grey.Lighten3).Padding(6).Text(notes);
                    }
                });
                p.Footer().AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });
        return doc.GeneratePdf();
    }
}
