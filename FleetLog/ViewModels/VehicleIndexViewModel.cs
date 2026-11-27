namespace FleetLog.ViewModels;

public class VehicleIndexViewModel
{
    public List<VehicleListViewModel> Vehicles { get; set; } = new();

    public string Search { get; set; } = string.Empty;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalCount { get; set; }

    public int TotalPages =>
        TotalCount == 0
            ? 1
            : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}