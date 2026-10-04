using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Data.Models;

public class FuelEntry
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }
    
    
    public DateTime Date { get; set; }
    
    [Precision(10, 2)]
    [Range(0.01, 1000)]
    public decimal Litres { get; set; }
    
    [Precision(10, 2)]
    [Range(typeof(decimal), "0", "100000")]
    public decimal TotalCost { get; set; }
    
    [Range(0, int.MaxValue)]
    public int MileageKm { get; set; }
    
    public virtual Vehicle Vehicle { get; set; } = null!;
    
}