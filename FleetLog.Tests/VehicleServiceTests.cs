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

    [Fact]
public async Task GetPageAsync_HandlesEmptyDatabase()
{
    var result = await _service.GetPageAsync(null);

    Assert.Empty(result.Vehicles);
    Assert.Equal(0, result.TotalCount);
    Assert.Equal(1, result.PageNumber);
    Assert.Equal(1, result.TotalPages);
    Assert.False(result.HasPreviousPage);
    Assert.False(result.HasNextPage);
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public async Task GetPageAsync_BlankSearchReturnsAllVehicles(string? search)
{
    await AddSearchVehiclesAsync();

    var result = await _service.GetPageAsync(search);

    Assert.Equal(string.Empty, result.Search);
    Assert.Equal(14, result.TotalCount);
    Assert.Equal(10, result.Vehicles.Count);
    Assert.Equal(2, result.TotalPages);
    Assert.False(result.HasPreviousPage);
    Assert.True(result.HasNextPage);
}

[Fact]
public async Task GetPageAsync_TrimsSearchAndMatchesIgnoringCase()
{
    await AddSearchVehiclesAsync();

    var result = await _service.GetPageAsync("  b0001  ");

    Assert.Equal("b0001", result.Search);
    Assert.Equal(1, result.TotalCount);

    var vehicle = Assert.Single(result.Vehicles);
    Assert.Equal("CB0001AA", vehicle.RegistrationNumber);
}

[Fact]
public async Task GetPageAsync_SearchSpansPagesInStableOrder()
{
    await AddSearchVehiclesAsync();

    var first = await _service.GetPageAsync("CB", 1);
    var second = await _service.GetPageAsync("CB", 2);
    var repeated = await _service.GetPageAsync("CB", 1);

    Assert.Equal(12, first.TotalCount);
    Assert.Equal(12, second.TotalCount);
    Assert.Equal(2, first.TotalPages);
    Assert.Equal(10, first.Vehicles.Count);
    Assert.Equal(2, second.Vehicles.Count);
    Assert.Equal("CB", second.Search);
    Assert.Equal(2, second.PageNumber);
    Assert.True(second.HasPreviousPage);
    Assert.False(second.HasNextPage);

    var registrations = first.Vehicles
        .Concat(second.Vehicles)
        .Select(v => v.RegistrationNumber)
        .ToArray();

    var expected = Enumerable.Range(1, 12)
        .Select(number => $"CB{number:D4}AA")
        .ToArray();

    Assert.Equal(expected, registrations);

    Assert.Equal(
        first.Vehicles.Select(v => v.Id).ToArray(),
        repeated.Vehicles.Select(v => v.Id).ToArray());

    Assert.Equal(
        12,
        first.Vehicles.Concat(second.Vehicles)
            .Select(v => v.Id)
            .Distinct()
            .Count());
}

[Theory]
[InlineData(-5, 1)]
[InlineData(0, 1)]
[InlineData(int.MaxValue, 2)]
public async Task GetPageAsync_ClampsInvalidPageNumbers(
    int requestedPage,
    int expectedPage)
{
    await AddSearchVehiclesAsync();

    var result = await _service.GetPageAsync("CB", requestedPage);

    Assert.Equal(expectedPage, result.PageNumber);
    Assert.Equal(12, result.TotalCount);
    Assert.Equal(expectedPage == 1 ? 10 : 2, result.Vehicles.Count);
    Assert.Equal(
        expectedPage == 1 ? "CB0001AA" : "CB0011AA",
        result.Vehicles[0].RegistrationNumber);
}

[Fact]
public async Task GetPageAsync_NoMatchesReturnsEmptyFirstPage()
{
    await AddSearchVehiclesAsync();

    var result = await _service.GetPageAsync("ZZ9999", 20);

    Assert.Empty(result.Vehicles);
    Assert.Equal("ZZ9999", result.Search);
    Assert.Equal(0, result.TotalCount);
    Assert.Equal(1, result.PageNumber);
    Assert.Equal(1, result.TotalPages);
    Assert.False(result.HasPreviousPage);
    Assert.False(result.HasNextPage);
}

private async Task AddSearchVehiclesAsync()
{
    // Insert in reverse order so insertion order differs from display order.
    for (var number = 14; number >= 1; number--)
    {
        var prefix = number <= 12 ? "CB" : "CA";

        _context.Vehicles.Add(new Vehicle
        {
            RegistrationNumber = $"{prefix}{number:D4}AA",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            VIN = $"TEST{number:D13}",
            FuelType = FuelType.Petrol,
            CurrentMileageKm = 10000,
            IsActive = true
        });
    }

    await _context.SaveChangesAsync();
    _context.ChangeTracker.Clear();
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