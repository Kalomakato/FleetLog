namespace FleetLog.ViewModels;

public class FuelEntryListViewModel
{
    public int Id { get; set; }
    
    public DateTime Date { get; set; }
    
    public decimal Litres { get; set; }
    
    public decimal TotalCost { get; set; }
    
    public int MileageKm { get; set; }
    
}