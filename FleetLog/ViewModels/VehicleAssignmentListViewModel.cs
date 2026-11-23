namespace FleetLog.ViewModels;

public class VehicleAssignmentListViewModel
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string DriverName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}