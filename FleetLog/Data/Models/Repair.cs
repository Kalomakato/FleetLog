using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetLog.Data.Models;

public class Repair
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey(nameof(Vehicle))]
    public int VehicleId { get; set; }
    
    public DateTime Date { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(typeof(decimal), "0", "100000")]
    public decimal TotalCost { get; set; }
    
    
    [Range(0, int.MaxValue)]
    public int MileageKm { get; set; }
    
    [MaxLength(200)]
    public string? ServiceCentre  { get; set; }
    
    public virtual Vehicle Vehicle { get; set; } = null!;
}