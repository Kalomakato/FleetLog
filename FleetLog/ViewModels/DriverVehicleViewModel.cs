namespace FleetLog.ViewModels;

public class DriverVehicleViewModel
{
    public int VehicleId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int CurrentMileageKm { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}