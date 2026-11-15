using FleetLog.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class VehicleService
{
    private readonly FleetLogDbContext _context;

    public VehicleService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleDuplicateCheckResult> CheckDuplicatesAsync(
        string registrationNumber,
        string vin,
        int? excludedVehicleId = null)
    {
        var vehicles = _context.Vehicles.AsNoTracking();

        if (excludedVehicleId.HasValue)
        {
            vehicles = vehicles.Where(
                v => v.Id != excludedVehicleId.Value);
        }

        bool registrationExists = await vehicles.AnyAsync(
            v => v.RegistrationNumber == registrationNumber);

        bool vinExists = await vehicles.AnyAsync(
            v => v.VIN == vin);

        return new VehicleDuplicateCheckResult(
            registrationExists,
            vinExists);
    }

    public async Task<bool> HasHistoryAsync(int vehicleId)
    {
        bool hasFuelEntries = await _context.FuelEntries
            .AnyAsync(f => f.VehicleId == vehicleId);

        bool hasRepairs = await _context.Repairs
            .AnyAsync(r => r.VehicleId == vehicleId);

        return hasFuelEntries || hasRepairs;
    }
}

public record VehicleDuplicateCheckResult(
    bool RegistrationExists,
    bool VinExists);