using Microsoft.EntityFrameworkCore;
using FleetLog.Data.Models;

namespace FleetLog.Data;

public class FleetLogDbContext : DbContext
{
    public FleetLogDbContext(DbContextOptions<FleetLogDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<Vehicle> Vehicles { get; set; }
    
    public DbSet<Repair> Repairs { get; set; }
    
    public DbSet<FuelEntry> FuelEntries { get; set; }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();
        
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.VIN)
            .IsUnique();
        
        modelBuilder.Entity<FuelEntry>()
            .HasOne(f => f.Vehicle)
            .WithMany(v => v.FuelEntries)
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Repair>()
            .HasOne(r => r.Vehicle)
            .WithMany(v => v.Repairs)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}