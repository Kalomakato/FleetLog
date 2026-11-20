using Microsoft.EntityFrameworkCore;
using FleetLog.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FleetLog.Data;

public class FleetLogDbContext : IdentityDbContext<IdentityUser>
{
    public FleetLogDbContext(DbContextOptions<FleetLogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }

    public DbSet<Repair> Repairs { get; set; }

    public DbSet<FuelEntry> FuelEntries { get; set; }

    public DbSet<Driver> Drivers { get; set; }

    public DbSet<VehicleAssignment> VehicleAssignments { get; set; }

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

        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithOne()
            .HasForeignKey<Driver>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleAssignment>()
            .HasOne(a => a.Driver)
            .WithMany(d => d.VehicleAssignments)
            .HasForeignKey(a => a.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleAssignment>()
            .HasOne(a => a.Vehicle)
            .WithMany(v => v.VehicleAssignments)
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}