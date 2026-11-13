using FleetLog.Data;
using FleetLog.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace FleetLog.Services;

public class FuelEntryService
{
    private readonly FleetLogDbContext _context;

    public FuelEntryService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<FuelEntryCreateResult> CreateAsync(
        int vehicleId, DateTime date, decimal litres, decimal totalCost, int mileageKm)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);

        if (vehicle is null)
        {
            return new FuelEntryCreateResult(VehicleFound: false);
        }

        bool futureDate = date.Date > DateTime.Today;
        bool mileageTooLow = !MileageRules.IsReadingValid(
            vehicle.CurrentMileageKm, mileageKm);

        if (futureDate || mileageTooLow)
        {
            return new FuelEntryCreateResult(
                VehicleFound: true,
                FutureDate: futureDate,
                MileageTooLow: mileageTooLow);
        }

        var entry = new FuelEntry
        {
            VehicleId = vehicle.Id,
            Date = date.Date,
            Litres = litres,
            TotalCost = totalCost,
            MileageKm = mileageKm
        };

        _context.FuelEntries.Add(entry);
        vehicle.CurrentMileageKm = mileageKm;
        await _context.SaveChangesAsync();

        return new FuelEntryCreateResult(VehicleFound: true);
    }
}

public record FuelEntryCreateResult(
    bool VehicleFound,
    bool FutureDate = false,
    bool MileageTooLow = false);