using System.Security.Claims;
using FleetLog.Data;
using FleetLog.Data.Enums;
using FleetLog.Data.Models;
using FleetLog.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FleetLog.Tests;

public class VehicleAccessServiceTests : IDisposable
{
    private const string OwnerId = "vehicle-owner";
    private const string OtherUserId = "other-driver";

    private readonly SqliteConnection _connection;
    private readonly FleetLogDbContext _context;
    private readonly VehicleAccessService _service;
    private readonly int _vehicleId;
    private readonly int _driverId;
    private readonly DateTime _today = DateTime.Today;

    public VehicleAccessServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FleetLogDbContext(options);
        _context.Database.EnsureCreated();

        var owner = new IdentityUser
        {
            Id = OwnerId,
            UserName = "owner@example.com",
            Email = "owner@example.com"
        };

        var otherUser = new IdentityUser
        {
            Id = OtherUserId,
            UserName = "other@example.com",
            Email = "other@example.com"
        };

        var driver = new Driver
        {
            FullName = "Assigned Driver",
            User = owner,
            IsActive = true
        };

        var otherDriver = new Driver
        {
            FullName = "Other Driver",
            User = otherUser,
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

        _context.Drivers.AddRange(driver, otherDriver);
        _context.Vehicles.Add(vehicle);
        _context.SaveChanges();

        _driverId = driver.Id;
        _vehicleId = vehicle.Id;
        _context.ChangeTracker.Clear();

        _service = new VehicleAccessService(_context);
    }

    [Fact]
    public async Task CanAccessAsync_RejectsUnauthenticatedUserEvenWithClaims()
    {
        await AddAssignmentAsync(_today);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, OwnerId),
            new Claim(ClaimTypes.Role, RoleNames.Administrator)
        }));

        Assert.False(await _service.CanAccessAsync(user, _vehicleId));
    }

    [Fact]
    public async Task CanAccessAsync_RejectsAuthenticatedUserWithoutAccountId()
    {
        await AddAssignmentAsync(_today);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            Array.Empty<Claim>(),
            "Test"));

        Assert.False(await _service.CanAccessAsync(user, _vehicleId));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CanAccessAsync_AllowsAdministratorWithoutAssignment(
        bool vehicleActive)
    {
        var vehicle = await _context.Vehicles.FindAsync(_vehicleId);
        vehicle!.IsActive = vehicleActive;
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var admin = CreateUser("admin-user", RoleNames.Administrator);

        Assert.True(await _service.CanAccessAsync(admin, _vehicleId));
    }

    [Fact]
    public async Task CanAccessAsync_RejectsMissingVehicleForAdministrator()
    {
        var admin = CreateUser("admin-user", RoleNames.Administrator);

        Assert.False(await _service.CanAccessAsync(admin, int.MaxValue));
    }

    [Fact]
    public async Task CanAccessAsync_RejectsDriverWithoutAssignment()
    {
        var user = CreateUser(OwnerId);

        Assert.False(await _service.CanAccessAsync(user, _vehicleId));
    }

    [Fact]
    public async Task CanAccessAsync_RejectsAnotherDriversVehicle()
    {
        await AddAssignmentAsync(_today.AddDays(-1));

        Assert.True(await _service.CanAccessAsync(
            CreateUser(OwnerId), _vehicleId));

        Assert.False(await _service.CanAccessAsync(
            CreateUser(OtherUserId), _vehicleId));
    }

    [Fact]
    public async Task CanAccessAsync_RejectsDifferentVehicleId()
    {
        await AddAssignmentAsync(_today);

        var otherVehicle = new Vehicle
        {
            RegistrationNumber = "CB2222BB",
            Make = "Ford",
            Model = "Focus",
            Year = 2020,
            VIN = "TEST0000000000002",
            FuelType = FuelType.Petrol,
            CurrentMileageKm = 50000,
            IsActive = true
        };

        _context.Vehicles.Add(otherVehicle);
        await _context.SaveChangesAsync();

        var otherVehicleId = otherVehicle.Id;
        _context.ChangeTracker.Clear();

        var user = CreateUser(OwnerId);

        Assert.True(await _service.CanAccessAsync(user, _vehicleId));
        Assert.False(await _service.CanAccessAsync(user, otherVehicleId));
    }

    [Theory]
    [InlineData(-1, null, true)]
    [InlineData(0, null, true)]
    [InlineData(1, null, false)]
    [InlineData(-2, -1, false)]
    [InlineData(-2, 0, true)]
    [InlineData(0, 0, true)]
    [InlineData(-2, 1, true)]
    [InlineData(1, 2, false)]
    public async Task CanAccessAsync_RespectsInclusiveAssignmentDates(
        int startOffset,
        int? endOffset,
        bool expected)
    {
        await AddAssignmentAsync(
            _today.AddDays(startOffset),
            endOffset.HasValue
                ? _today.AddDays(endOffset.Value)
                : null);

        var result = await _service.CanAccessAsync(
            CreateUser(OwnerId), _vehicleId);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task CanAccessAsync_RejectsInactiveDriverOrVehicle(
        bool driverActive,
        bool vehicleActive)
    {
        await AddAssignmentAsync(_today);

        var driver = await _context.Drivers.FindAsync(_driverId);
        var vehicle = await _context.Vehicles.FindAsync(_vehicleId);

        driver!.IsActive = driverActive;
        vehicle!.IsActive = vehicleActive;

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        Assert.False(await _service.CanAccessAsync(
            CreateUser(OwnerId), _vehicleId));
    }

    private async Task AddAssignmentAsync(
        DateTime startDate,
        DateTime? endDate = null)
    {
        _context.VehicleAssignments.Add(new VehicleAssignment
        {
            DriverId = _driverId,
            VehicleId = _vehicleId,
            StartDate = startDate,
            EndDate = endDate
        });

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }

    private static ClaimsPrincipal CreateUser(
        string userId,
        string role = RoleNames.User)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        }, "Test");

        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}