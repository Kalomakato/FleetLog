using System.Security.Claims;
using FleetLog.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class VehicleAccessService
{
    private readonly FleetLogDbContext _context;

    public VehicleAccessService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessAsync(
        ClaimsPrincipal user,
        int vehicleId)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(RoleNames.Administrator))
        {
            return await _context.Vehicles
                .AnyAsync(v => v.Id == vehicleId);
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var today = DateTime.Today;

        return await _context.VehicleAssignments
            .AsNoTracking()
            .AnyAsync(a =>
                a.VehicleId == vehicleId &&
                a.Driver.UserId == userId &&
                a.Driver.IsActive &&
                a.Vehicle.IsActive &&
                a.StartDate <= today &&
                (!a.EndDate.HasValue || a.EndDate.Value >= today));
    }
}
