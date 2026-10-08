using FleetLog.Data.Enums;

namespace FleetLog.ViewModels;

public class VehicleDetailsViewModel
{
    public int Id { get; set; }
    
    public string RegistrationNumber { get; set; } =  string.Empty;

    public string Make { get; set; } = string.Empty;
    
    public string Model { get; set; } = string.Empty;
    
    public int Year { get; set; }
    
    public int CurrentMileageKm { get; set; }
    
    public bool IsActive { get; set; }
    
    public string VIN { get; set; } = string.Empty;
    
    public FuelType FuelType { get; set; }
}