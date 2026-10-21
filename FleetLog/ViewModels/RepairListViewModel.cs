namespace FleetLog.ViewModels;

public class RepairListViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }  = string.Empty;
    public decimal TotalCost { get; set; }
    public int MileageKm { get; set; }
    public string? ServiceCentre { get; set; }
}