using FleetLog.Data;
using FleetLog.Data.Models;

namespace FleetLog.Services;

public class RepairService
{
    private readonly FleetLogDbContext _context;

    public RepairService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<RepairCreateResult> CreateAsync(
        int vehicleId,
        DateTime date,
        string description,
        decimal totalCost,
        int mileageKm,
        string? serviceCentre)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);

        if (vehicle is null)
        {
            return new RepairCreateResult(VehicleFound: false);
        }

        bool futureDate = date.Date > DateTime.Today;
        bool mileageTooLow = !MileageRules.IsReadingValid(
            vehicle.CurrentMileageKm, mileageKm);

        if (futureDate || mileageTooLow)
        {
            return new RepairCreateResult(
                VehicleFound: true,
                FutureDate: futureDate,
                MileageTooLow: mileageTooLow);
        }

        var repair = new Repair
        {
            VehicleId = vehicle.Id,
            Date = date.Date,
            Description = description.Trim(),
            TotalCost = totalCost,
            MileageKm = mileageKm,
            ServiceCentre = string.IsNullOrWhiteSpace(serviceCentre)
                ? null
                : serviceCentre.Trim()
        };

        _context.Repairs.Add(repair);
        vehicle.CurrentMileageKm = mileageKm;

        await _context.SaveChangesAsync();

        return new RepairCreateResult(VehicleFound: true);
    }
}

public record RepairCreateResult(
    bool VehicleFound,
    bool FutureDate = false,
    bool MileageTooLow = false);