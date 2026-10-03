using System.ComponentModel.DataAnnotations;
using FleetLog.Data.Enums;

namespace FleetLog.Data.Models;

public class Vehicle
{
    [Key]
    public int Id { get; set; } 
    
    [Required]
    [MaxLength(8)]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Make { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;
    
    
    [Range(1900, 2026)]
    public int Year { get; set; }
    
    [Required]
    [StringLength(17, MinimumLength = 17)]
    public string VIN { get; set; } = string.Empty;
    
    [EnumDataType(typeof(FuelType))]
    public FuelType FuelType { get; set; }
    
    [Range(0, int.MaxValue)]
    public int CurrentMileageKm { get; set; }
    
    
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
    
    public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}