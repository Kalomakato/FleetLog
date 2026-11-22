using FleetLog.Data;
using FleetLog.Data.Enums;
using FleetLog.Data.Models;
using FleetLog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FleetLog.Tests;

public class VehicleAssignmentServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FleetLogDbContext _context;
    private readonly VehicleAssignmentService _service;
    private readonly int _vehicleId;
    private readonly int _driverId;

    private static readonly DateTime StartDate = new(2026, 11, 22);

    public VehicleAssignmentServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FleetLogDbContext(options);
        _context.Database.EnsureCreated();

        var user = new IdentityUser
        {
            Id = "assignment-test-user",
            UserName = "driver@example.com",
            Email = "driver@example.com"
        };

        var driver = new Driver
        {
            FullName = "Test Driver",
            User = user,
            IsActive = true
        };

        var vehicle = new Vehicle
        {
            RegistrationNumber = "CB1111AA",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            VIN = "TEST0000000000001",
            FuelType = FuelType.Petrol,
            CurrentMileageKm = 86220,
            IsActive = true
        };

        _context.Drivers.Add(driver);
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();

        _driverId = driver.Id;
        _vehicleId = vehicle.Id;
        _context.ChangeTracker.Clear();

        _service = new VehicleAssignmentService(_context);
    }

    [Fact]
    public async Task AssignAsync_SavesOpenAssignment()
    {
        var result = await _service.AssignAsync(
            _vehicleId, _driverId, StartDate);

        Assert.Equal(VehicleAssignmentResult.Success, result);

        _context.ChangeTracker.Clear();
        var saved = await _context.VehicleAssignments.SingleAsync();

        Assert.Equal(_vehicleId, saved.VehicleId);
        Assert.Equal(_driverId, saved.DriverId);
        Assert.Equal(StartDate, saved.StartDate);
        Assert.Null(saved.EndDate);
    }

    [Fact]
    public async Task AssignAsync_RejectsOverlapWithOpenAssignment()
    {
        await AddAssignmentAsync(StartDate);

        var result = await _service.AssignAsync(
            _vehicleId, _driverId, StartDate.AddDays(1));

        Assert.Equal(
            VehicleAssignmentResult.OverlappingAssignment, result);

        _context.ChangeTracker.Clear();
        Assert.Equal(1, await _context.VehicleAssignments.CountAsync());
    }

    [Theory]
    [InlineData(-2, -1, false)]
    [InlineData(-1, 0, true)]
    [InlineData(0, 0, true)]
    [InlineData(1, 2, true)]
    [InlineData(-1, 4, true)]
    [InlineData(3, 4, true)]
    [InlineData(4, 5, false)]
    public async Task AssignAsync_ChecksInclusiveDateOverlap(
        int startOffset,
        int endOffset,
        bool overlaps)
    {
        await AddAssignmentAsync(StartDate, StartDate.AddDays(3));

        var result = await _service.AssignAsync(
            _vehicleId,
            _driverId,
            StartDate.AddDays(startOffset),
            StartDate.AddDays(endOffset));

        Assert.Equal(
            overlaps
                ? VehicleAssignmentResult.OverlappingAssignment
                : VehicleAssignmentResult.Success,
            result);

        _context.ChangeTracker.Clear();
        Assert.Equal(
            overlaps ? 1 : 2,
            await _context.VehicleAssignments.CountAsync());
    }

    [Fact]
    public async Task AssignAsync_RejectsEndBeforeStart()
    {
        var result = await _service.AssignAsync(
            _vehicleId,
            _driverId,
            StartDate,
            StartDate.AddDays(-1));

        Assert.Equal(VehicleAssignmentResult.EndBeforeStart, result);

        _context.ChangeTracker.Clear();
        Assert.Empty(await _context.VehicleAssignments.ToListAsync());
    }

    [Fact]
    public async Task AssignAsync_AllowsSameStartAndEndDate()
    {
        var result = await _service.AssignAsync(
            _vehicleId, _driverId, StartDate, StartDate);

        Assert.Equal(VehicleAssignmentResult.Success, result);

        _context.ChangeTracker.Clear();
        var saved = await _context.VehicleAssignments.SingleAsync();

        Assert.Equal(StartDate, saved.StartDate);
        Assert.Equal(StartDate, saved.EndDate);
    }

    [Fact]
    public async Task ReleaseAsync_RejectsEndBeforeStartWithoutChangingRecord()
    {
        var assignmentId = await AddAssignmentAsync(StartDate);

        var result = await _service.ReleaseAsync(
            assignmentId, StartDate.AddDays(-1));

        Assert.Equal(VehicleAssignmentResult.EndBeforeStart, result);

        _context.ChangeTracker.Clear();
        var saved = await _context.VehicleAssignments.SingleAsync();

        Assert.Null(saved.EndDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ReleaseAsync_AcceptsEndOnOrAfterStart(int daysAfterStart)
    {
        var assignmentId = await AddAssignmentAsync(StartDate);
        var endDate = StartDate.AddDays(daysAfterStart);

        var result = await _service.ReleaseAsync(assignmentId, endDate);

        Assert.Equal(VehicleAssignmentResult.Success, result);

        _context.ChangeTracker.Clear();
        var saved = await _context.VehicleAssignments.SingleAsync();

        Assert.Equal(endDate, saved.EndDate);
    }

    [Fact]
    public async Task ReleaseAsync_AllowsReassignmentTheFollowingDay()
    {
        var assignmentId = await AddAssignmentAsync(StartDate);

        var releaseResult = await _service.ReleaseAsync(
            assignmentId, StartDate);

        var assignResult = await _service.AssignAsync(
            _vehicleId, _driverId, StartDate.AddDays(1));

        Assert.Equal(VehicleAssignmentResult.Success, releaseResult);
        Assert.Equal(VehicleAssignmentResult.Success, assignResult);

        _context.ChangeTracker.Clear();
        Assert.Equal(2, await _context.VehicleAssignments.CountAsync());
    }

    private async Task<int> AddAssignmentAsync(
        DateTime startDate,
        DateTime? endDate = null)
    {
        var assignment = new VehicleAssignment
        {
            VehicleId = _vehicleId,
            DriverId = _driverId,
            StartDate = startDate,
            EndDate = endDate
        };

        _context.VehicleAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        return assignment.Id;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}