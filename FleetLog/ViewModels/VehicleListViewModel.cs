namespace FleetLog.ViewModels;

public class VehicleListViewModel
{
    public int Id { get; set; }
    
    public string RegistrationNumber { get; set; } =  string.Empty;

    public string Make { get; set; } = string.Empty;
    
    public string Model { get; set; } = string.Empty;
    
    public int Year { get; set; }
    
    public int CurrentMileageKm { get; set; }
    
    public bool IsActive { get; set; }
}