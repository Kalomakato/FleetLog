using System.Security.Claims;
using FleetLog.Controllers;
using FleetLog.Data;
using FleetLog.Data.Enums;
using FleetLog.Data.Models;
using FleetLog.Services;
using FleetLog.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FleetLog.Tests;

public class SubmissionAccessTests : IDisposable
{
    private const string UserId = "submission-driver";
    private const int InitialMileage = 10000;
    private const int SubmittedMileage = 10100;

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FleetLogDbContext> _options;
    private readonly int _vehicleId;
    private readonly int _otherVehicleId;
    private readonly int _assignmentId;
    private readonly DateTime _today = DateTime.Today;

    public SubmissionAccessTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<FleetLogDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = NewContext();
        context.Database.EnsureCreated();

        var driver = new Driver
        {
            FullName = "Submission Driver",
            IsActive = true,
            User = new IdentityUser
            {
                Id = UserId,
                UserName = "submission@example.com",
                Email = "submission@example.com"
            }
        };

        var vehicle = CreateVehicle("CB1111AA", "TEST0000000000001");
        var otherVehicle = CreateVehicle("CB2222BB", "TEST0000000000002");

        var assignment = new VehicleAssignment
        {
            Driver = driver,
            Vehicle = vehicle,
            StartDate = _today.AddDays(-7)
        };

        context.VehicleAssignments.Add(assignment);
        context.Vehicles.Add(otherVehicle);
        context.SaveChanges();

        _vehicleId = vehicle.Id;
        _otherVehicleId = otherVehicle.Id;
        _assignmentId = assignment.Id;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateGet_RejectsUnassignedVehicle(bool repair)
    {
        using var context = NewContext();

        var result = await OpenFormAsync(
            context, repair, _otherVehicleId);

        Assert.IsType<NotFoundResult>(result);
        await AssertNothingChangedAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreatePost_RejectsChangedVehicleId(bool repair)
    {
        using (var getContext = NewContext())
        {
            var form = await OpenFormAsync(
                getContext, repair, _vehicleId);

            Assert.IsType<ViewResult>(form);
        }

        // The form was opened for the assigned vehicle,
        // but the submitted hidden ID targets another vehicle.
        using (var postContext = NewContext())
        {
            var result = await SubmitAsync(
                postContext, repair, _otherVehicleId);

            Assert.IsType<NotFoundResult>(result);
        }

        await AssertNothingChangedAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreatePost_RechecksAssignmentAfterFormWasOpened(
        bool repair)
    {
        using (var getContext = NewContext())
        {
            var form = await OpenFormAsync(
                getContext, repair, _vehicleId);

            Assert.IsType<ViewResult>(form);
        }

        await ReleaseAssignmentAsync();

        using (var postContext = NewContext())
        {
            var result = await SubmitAsync(
                postContext, repair, _vehicleId);

            Assert.IsType<NotFoundResult>(result);
        }

        await AssertNothingChangedAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateGet_RejectsEndedAssignment(bool repair)
    {
        await ReleaseAssignmentAsync();

        using var context = NewContext();

        var result = await OpenFormAsync(
            context, repair, _vehicleId);

        Assert.IsType<NotFoundResult>(result);
        await AssertNothingChangedAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreatePost_SavesForCurrentlyAssignedDriver(bool repair)
    {
        using (var getContext = NewContext())
        {
            Assert.IsType<ViewResult>(
                await OpenFormAsync(getContext, repair, _vehicleId));
        }

        using (var postContext = NewContext())
        {
            var result = await SubmitAsync(
                postContext, repair, _vehicleId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        using var verification = NewContext();

        Assert.Equal(
            repair ? 0 : 1,
            await verification.FuelEntries.CountAsync());

        Assert.Equal(
            repair ? 1 : 0,
            await verification.Repairs.CountAsync());

        if (repair)
        {
            var saved = await verification.Repairs.SingleAsync();
            Assert.Equal(_vehicleId, saved.VehicleId);
            Assert.Equal(SubmittedMileage, saved.MileageKm);
            Assert.Equal("Oil change", saved.Description);
        }
        else
        {
            var saved = await verification.FuelEntries.SingleAsync();
            Assert.Equal(_vehicleId, saved.VehicleId);
            Assert.Equal(SubmittedMileage, saved.MileageKm);
            Assert.Equal(20m, saved.Litres);
        }

        Assert.Equal(
            SubmittedMileage,
            await verification.Vehicles
                .Where(v => v.Id == _vehicleId)
                .Select(v => v.CurrentMileageKm)
                .SingleAsync());

        Assert.Equal(
            InitialMileage,
            await verification.Vehicles
                .Where(v => v.Id == _otherVehicleId)
                .Select(v => v.CurrentMileageKm)
                .SingleAsync());
    }

    private async Task ReleaseAssignmentAsync()
    {
        using var context = NewContext();
        var service = new VehicleAssignmentService(context);

        // End dates are inclusive, so yesterday means access has ended.
        var result = await service.ReleaseAsync(
            _assignmentId, _today.AddDays(-1));

        Assert.Equal(VehicleAssignmentResult.Success, result);
    }

    private async Task AssertNothingChangedAsync()
    {
        using var verification = NewContext();

        Assert.Equal(0, await verification.FuelEntries.CountAsync());
        Assert.Equal(0, await verification.Repairs.CountAsync());

        var mileages = await verification.Vehicles
            .Select(v => v.CurrentMileageKm)
            .ToListAsync();

        Assert.Equal(2, mileages.Count);
        Assert.All(mileages, mileage =>
            Assert.Equal(InitialMileage, mileage));
    }

    private Task<IActionResult> OpenFormAsync(
        FleetLogDbContext context,
        bool repair,
        int vehicleId)
    {
        return repair
            ? CreateRepairsController(context).Create(vehicleId)
            : CreateFuelController(context).Create(vehicleId);
    }

    private Task<IActionResult> SubmitAsync(
        FleetLogDbContext context,
        bool repair,
        int vehicleId)
    {
        if (repair)
        {
            return CreateRepairsController(context).Create(
                new RepairCreateViewModel
                {
                    VehicleId = vehicleId,
                    Date = _today,
                    Description = "Oil change",
                    TotalCost = 100m,
                    MileageKm = SubmittedMileage,
                    ServiceCentre = "Test garage"
                });
        }

        return CreateFuelController(context).Create(
            new FuelEntryCreateViewModel
            {
                VehicleId = vehicleId,
                Date = _today,
                Litres = 20m,
                TotalCost = 50m,
                MileageKm = SubmittedMileage
            });
    }

    private FuelEntriesController CreateFuelController(
        FleetLogDbContext context)
    {
        return new FuelEntriesController(
            context,
            new FuelEntryService(context),
            new VehicleAccessService(context))
        {
            ControllerContext = CreateControllerContext()
        };
    }

    private RepairsController CreateRepairsController(
        FleetLogDbContext context)
    {
        return new RepairsController(
            context,
            new RepairService(context),
            new VehicleAccessService(context))
        {
            ControllerContext = CreateControllerContext()
        };
    }

    private static ControllerContext CreateControllerContext()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Role, RoleNames.User)
        }, "Test");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private FleetLogDbContext NewContext()
    {
        return new FleetLogDbContext(_options);
    }

    private static Vehicle CreateVehicle(
        string registrationNumber,
        string vin)
    {
        return new Vehicle
        {
            RegistrationNumber = registrationNumber,
            Make = "Toyota",
            Model = "Corolla",
            Year = 2020,
            VIN = vin,
            FuelType = FuelType.Petrol,
            CurrentMileageKm = InitialMileage,
            IsActive = true
        };
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}