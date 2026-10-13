namespace FleetLog.ViewModels;

public class VehicleDeleteViewModel
{
    public int Id { get; set; }
    
    public string RegistrationNumber { get; set; } =  string.Empty;

    public string Make { get; set; } = string.Empty;
    
    public string Model { get; set; } = string.Empty;
}