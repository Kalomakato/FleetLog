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
}