using FleetLog.Data;
using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class DriverVehicleService
{
    private readonly FleetLogDbContext _context;

    public DriverVehicleService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<List<DriverVehicleViewModel>> GetCurrentVehiclesAsync(
        string userId,
        DateTime date)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<DriverVehicleViewModel>();
        }

        var day = date.Date;

        return await _context.VehicleAssignments
            .AsNoTracking()
            .Where(a => a.Driver.UserId == userId)
            .Where(a => a.Driver.IsActive && a.Vehicle.IsActive)
            .Where(a => a.StartDate <= day &&
                        (!a.EndDate.HasValue || a.EndDate.Value >= day))
            .OrderBy(a => a.Vehicle.RegistrationNumber)
            .ThenBy(a => a.Id)
            .Select(a => new DriverVehicleViewModel
            {
                VehicleId = a.VehicleId,
                RegistrationNumber = a.Vehicle.RegistrationNumber,
                Make = a.Vehicle.Make,
                Model = a.Vehicle.Model,
                CurrentMileageKm = a.Vehicle.CurrentMileageKm,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync();
    }
}