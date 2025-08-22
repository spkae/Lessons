using AutoService.Models;
using System.ComponentModel.DataAnnotations;
public class Vehicle
{
    public int Id { get; set; }
    [Required] public string Make { get; set; } = "";
    [Required] public string Model { get; set; } = "";
    [Range(1900, 2100)] public int Year { get; set; }
    public string? Vin { get; set; }
    public string? Plate { get; set; }
    [Required] public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<JobOrder> Jobs { get; set; } = new List<JobOrder>();
}
