
using System.Text.Json;
using AutoService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoService.Services;

public class WorkSheetPdfService
{
    public WorkSheetPdfService(){ QuestPDF.Settings.License = LicenseType.Community; }

    public Task<byte[]> Render(FormEntry entry)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string,string>>(entry.DataJson) ?? new();

        var doc = Document.Create(c =>
        {
            c.Page(p =>
            {
                p.Margin(25);
                p.Header().Text("Work Sheet").SemiBold().FontSize(16);
                p.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Table(t => {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(); cd.RelativeColumn(); cd.RelativeColumn(); });
                        t.Cell().Text($"Customer: {entry.Vehicle?.Customer?.FullName}");
                        t.Cell().Text($"VIN: {entry.Vehicle?.Vin}");
                        t.Cell().Text($"Plate: {entry.Vehicle?.Plate}");
                    });

                    for (int i=1;i<=10;i++)
                    {
                        var key = $"task_{i}";
                        if (!data.TryGetValue(key, out var val) || string.IsNullOrWhiteSpace(val)) continue;
                        col.Item().Text($"Task {i}: {val}");
                    }

                    col.Item().Text($"Hours: {data.GetValueOrDefault("hours","")}");
                    if (data.TryGetValue("notes", out var notes) && !string.IsNullOrWhiteSpace(notes))
                    {
                        col.Item().Text("Notes:").SemiBold();
                        col.Item().Background(Colors.Grey.Lighten3).Padding(6).Text(notes);
                    }
                });
                p.Footer().AlignRight().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            });
        });
        return Task.FromResult(doc.GeneratePdf());
    }
}
