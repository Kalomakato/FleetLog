using FleetLog.Data;
using FleetLog.Data.Enums;
using FleetLog.Data.Models;
using FleetLog.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FleetLog.Tests;

public class VehicleServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FleetLogDbContext _context;
    private readonly VehicleService _service;

    public VehicleServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FleetLogDbContext(options);
        _context.Database.EnsureCreated();

        _service = new VehicleService(_context);
    }

    [Fact]
    public async Task CheckDuplicatesAsync_AllowsUnchangedVehicle()
    {
        var vehicle = await AddVehicleAsync();

        var result = await _service.CheckDuplicatesAsync(
            vehicle.RegistrationNumber,
            vehicle.VIN,
            excludedVehicleId: vehicle.Id);

        Assert.False(result.RegistrationExists);
        Assert.False(result.VinExists);
    }

    [Theory]
    [InlineData("CB1111AA", "TEST0000000000002", true, false)]
    [InlineData("CB2222AA", "TEST0000000000001", false, true)]
    [InlineData("CB1111AA", "TEST0000000000001", true, true)]
    [InlineData("CB2222AA", "TEST0000000000002", false, false)]
    public async Task CheckDuplicatesAsync_DetectsExistingFields(
        string registrationNumber,
        string vin,
        bool expectedRegistration,
        bool expectedVin)
    {
        await AddVehicleAsync();

        var result = await _service.CheckDuplicatesAsync(
            registrationNumber, vin);

        Assert.Equal(expectedRegistration, result.RegistrationExists);
        Assert.Equal(expectedVin, result.VinExists);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task HasHistoryAsync_DetectsFuelOrRepairs(
        bool addFuel,
        bool addRepair,
        bool expectedHistory)
    {
        var vehicle = await AddVehicleAsync();

        if (addFuel)
        {
            _context.FuelEntries.Add(new FuelEntry
            {
                VehicleId = vehicle.Id,
                Date = DateTime.Today,
                Litres = 10m,
                TotalCost = 25m,
                MileageKm = 86220
            });
        }

        if (addRepair)
        {
            _context.Repairs.Add(new Repair
            {
                VehicleId = vehicle.Id,
                Date = DateTime.Today,
                Description = "Смяна на масло",
                TotalCost = 150m,
                MileageKm = 86220
            });
        }

        await _context.SaveChangesAsync();

        var result = await _service.HasHistoryAsync(vehicle.Id);

        Assert.Equal(expectedHistory, result);
    }

    private async Task<Vehicle> AddVehicleAsync()
    {
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

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return vehicle;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}