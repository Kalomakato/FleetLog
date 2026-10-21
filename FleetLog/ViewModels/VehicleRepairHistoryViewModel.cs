namespace FleetLog.ViewModels;

public class VehicleRepairHistoryViewModel
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public List<RepairListViewModel> Repairs { get; set; } = new();
}