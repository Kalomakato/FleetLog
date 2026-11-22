using System.Data;
using FleetLog.Data;
using FleetLog.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class VehicleAssignmentService
{
    private readonly FleetLogDbContext _context;

    public VehicleAssignmentService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<VehicleAssignmentResult> AssignAsync(
        int vehicleId,
        int driverId,
        DateTime startDate,
        DateTime? endDate = null)
    {
        startDate = startDate.Date;
        endDate = endDate?.Date;

        if (endDate.HasValue && endDate.Value < startDate)
        {
            return VehicleAssignmentResult.EndBeforeStart;
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var vehicle = await _context.Vehicles
            .SingleOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle is null)
        {
            return VehicleAssignmentResult.VehicleNotFound;
        }

        if (!vehicle.IsActive)
        {
            return VehicleAssignmentResult.VehicleInactive;
        }

        var driver = await _context.Drivers
            .SingleOrDefaultAsync(d => d.Id == driverId);

        if (driver is null)
        {
            return VehicleAssignmentResult.DriverNotFound;
        }

        if (!driver.IsActive)
        {
            return VehicleAssignmentResult.DriverInactive;
        }

        var assignments = _context.VehicleAssignments
            .Where(a => a.VehicleId == vehicleId)
            .Where(a => !a.EndDate.HasValue ||
                        a.EndDate.Value >= startDate);

        if (endDate.HasValue)
        {
            var lastDay = endDate.Value;

            assignments = assignments.Where(
                a => a.StartDate <= lastDay);
        }

        if (await assignments.AnyAsync())
        {
            return VehicleAssignmentResult.OverlappingAssignment;
        }

        _context.VehicleAssignments.Add(new VehicleAssignment
        {
            VehicleId = vehicleId,
            DriverId = driverId,
            StartDate = startDate,
            EndDate = endDate
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return VehicleAssignmentResult.Success;
    }

    public async Task<VehicleAssignmentResult> ReleaseAsync(
        int assignmentId,
        DateTime endDate)
    {
        endDate = endDate.Date;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var assignment = await _context.VehicleAssignments
            .SingleOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment is null)
        {
            return VehicleAssignmentResult.AssignmentNotFound;
        }

        if (assignment.EndDate.HasValue)
        {
            return VehicleAssignmentResult.AssignmentAlreadyEnded;
        }

        if (endDate < assignment.StartDate)
        {
            return VehicleAssignmentResult.EndBeforeStart;
        }

        assignment.EndDate = endDate;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return VehicleAssignmentResult.Success;
    }
}

public enum VehicleAssignmentResult
{
    Success,
    VehicleNotFound,
    DriverNotFound,
    VehicleInactive,
    DriverInactive,
    AssignmentNotFound,
    AssignmentAlreadyEnded,
    EndBeforeStart,
    OverlappingAssignment
}