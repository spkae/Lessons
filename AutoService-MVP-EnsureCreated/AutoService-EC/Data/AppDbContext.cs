
using AutoService.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<JobOrder> JobOrders => Set<JobOrder>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<JobPart> JobParts => Set<JobPart>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<JobPart>().Property(p => p.UnitPrice).HasPrecision(18,2);
        mb.Entity<Part>().Property(p => p.UnitCost).HasPrecision(18,2);
        mb.Entity<Part>().Property(p => p.UnitPrice).HasPrecision(18,2);
        mb.Entity<JobOrder>().Property(p => p.LaborRate).HasPrecision(18,2);
        mb.Entity<JobOrder>().Property(p => p.LaborHours).HasPrecision(18,2);

        mb.Entity<JobPart>()
          .HasOne(jp => jp.JobOrder).WithMany(j => j.Parts)
          .HasForeignKey(jp => jp.JobOrderId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<JobPart>()
          .HasOne(jp => jp.Part).WithMany(p => p.JobParts)
          .HasForeignKey(jp => jp.PartId).OnDelete(DeleteBehavior.Restrict);

        // Seed demo
        mb.Entity<Customer>().HasData(new Customer{ Id=1, FullName="Иван Петров", Phone="709-555-0101"});
        mb.Entity<Vehicle>().HasData(new Vehicle{ Id=1, Make="Toyota", Model="Camry", Year=2015, Vin="123VIN", Plate="ABC123", CustomerId=1 });
        mb.Entity<Part>().HasData(
            new Part{ Id=1, Name="Масляный фильтр", Sku="OF-001", UnitCost=6.50m, UnitPrice=12.99m, StockQty=20 },
            new Part{ Id=2, Name="Моторное масло 5W-30 (1л)", Sku="OIL-5W30-1L", UnitCost=7.00m, UnitPrice=12.00m, StockQty=100 }
        );
        mb.Entity<JobOrder>().HasData(new JobOrder{ Id=1, VehicleId=1, Title="ТО-замена масла", Status=JobStatus.Planned, LaborRate=90m, LaborHours=0.5m, CreatedAt=DateTime.UtcNow });
        mb.Entity<JobPart>().HasData(
            new JobPart{ Id=1, JobOrderId=1, PartId=1, Quantity=1, UnitPrice=12.99m },
            new JobPart{ Id=2, JobOrderId=1, PartId=2, Quantity=4, UnitPrice=12.00m }
        );
    }
}
