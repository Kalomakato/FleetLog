namespace FleetLog.ViewModels;

public class VehicleFuelHistoryViewModel
{
    public int VehicleId { get; set; }
    
    public string RegistrationNumber { get; set; } = string.Empty;
    
    public List<FuelEntryListViewModel> FuelEntries { get; set; } = new();
    
}