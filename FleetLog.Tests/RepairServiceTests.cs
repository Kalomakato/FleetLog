using FleetLog.Data;
using FleetLog.Data.Enums;
using FleetLog.Data.Models;
using FleetLog.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FleetLog.Tests;

public class RepairServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsFutureDateWithoutSaving()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new FleetLogDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var vehicle = CreateVehicle();
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new RepairService(context);

        var result = await service.CreateAsync(
            vehicleId: vehicle.Id,
            date: DateTime.Today.AddDays(1),
            description: "Смяна на масло",
            totalCost: 150m,
            mileageKm: 86220,
            serviceCentre: null);

        Assert.True(result.VehicleFound);
        Assert.True(result.FutureDate);
        Assert.False(result.MileageTooLow);

        context.ChangeTracker.Clear();

        Assert.Empty(await context.Repairs.ToListAsync());

        var savedVehicle = await context.Vehicles.SingleAsync();
        Assert.Equal(86210, savedVehicle.CurrentMileageKm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_AcceptsBlankServiceCentre(
        string? serviceCentre)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new FleetLogDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var vehicle = CreateVehicle();
        context.Vehicles.Add(vehicle);
        await context.SaveChangesAsync();

        var service = new RepairService(context);

        var result = await service.CreateAsync(
            vehicleId: vehicle.Id,
            date: DateTime.Today,
            description: "Смяна на масло",
            totalCost: 150m,
            mileageKm: 86220,
            serviceCentre: serviceCentre);

        Assert.True(result.VehicleFound);
        Assert.False(result.FutureDate);
        Assert.False(result.MileageTooLow);

        context.ChangeTracker.Clear();

        var savedRepair = await context.Repairs.SingleAsync();

        Assert.Equal(vehicle.Id, savedRepair.VehicleId);
        Assert.Null(savedRepair.ServiceCentre);
        Assert.Equal(86220, savedRepair.MileageKm);
        Assert.Equal(150m, savedRepair.TotalCost);

        var savedVehicle = await context.Vehicles.SingleAsync();
        Assert.Equal(86220, savedVehicle.CurrentMileageKm);
    }

    private static Vehicle CreateVehicle()
    {
        return new Vehicle
        {
            RegistrationNumber = "CB1111AA",
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            VIN = "TEST0000000000001",
            FuelType = FuelType.Petrol,
            CurrentMileageKm = 86210,
            IsActive = true
        };
    }
}