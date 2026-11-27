using FleetLog.Data;
using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class VehicleService
{
    private readonly FleetLogDbContext _context;

    public VehicleService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleIndexViewModel> GetPageAsync(
        string? search,
        int pageNumber = 1)
    {
        const int pageSize = 10;

        var searchText = search?.Trim() ?? string.Empty;
        var vehicles = _context.Vehicles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearch = searchText.ToUpperInvariant();

            vehicles = vehicles.Where(v =>
                v.RegistrationNumber.ToUpper().Contains(normalizedSearch));
        }

        var totalCount = await vehicles.CountAsync();

        var totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling((double)totalCount / pageSize);

        pageNumber = Math.Clamp(pageNumber, 1, totalPages);

        var items = await vehicles
            .OrderBy(v => v.RegistrationNumber)
            .ThenBy(v => v.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VehicleListViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                CurrentMileageKm = v.CurrentMileageKm,
                IsActive = v.IsActive
            })
            .ToListAsync();

        return new VehicleIndexViewModel
        {
            Vehicles = items,
            Search = searchText,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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