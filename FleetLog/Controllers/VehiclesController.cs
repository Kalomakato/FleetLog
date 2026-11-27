using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;
using FleetLog.Data;
using FleetLog.Data.Models;
using FleetLog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;

namespace FleetLog.Controllers;

[Authorize]
public class VehiclesController : Controller
{
    private readonly FleetLogDbContext _context;
    private readonly VehicleService _vehicleService;
    private readonly VehicleAccessService _vehicleAccessService;

    public VehiclesController(
        FleetLogDbContext context,
        VehicleService vehicleService,
        VehicleAccessService vehicleAccessService)
    {
        _context = context;
        _vehicleService = vehicleService;
        _vehicleAccessService = vehicleAccessService;
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Index(
        string? search,
        int pageNumber = 1)
    {
        var model = await _vehicleService.GetPageAsync(search, pageNumber);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (!await _vehicleAccessService.CanAccessAsync(User, id))
        {
            return NotFound();
        }

        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleDetailsViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VIN = v.VIN,
                FuelType = v.FuelType,
                CurrentMileageKm = v.CurrentMileageKm,
                IsActive = v.IsActive
            })
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Administrator)]
    public IActionResult Create()
    {
        return View(new VehicleCreateViewModel());
    }

    // Empty prefix avoids a clash with the vehicle's Model field.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Create(
        [Bind(Prefix = "")] VehicleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await ValidateUniqueVehicleFieldsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var vehicle = new Vehicle
        {
            RegistrationNumber = model.RegistrationNumber,
            Make = model.Make,
            Model = model.Model,
            Year = model.Year,
            VIN = model.VIN,
            FuelType = model.FuelType!.Value,
            CurrentMileageKm = model.CurrentMileageKm,
            IsActive = model.IsActive
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleEditViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                VIN = v.VIN,
                FuelType = v.FuelType,
                CurrentMileageKm = v.CurrentMileageKm,
                IsActive = v.IsActive
            })
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Edit(
        [Bind(Prefix = "")] VehicleEditViewModel model)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == model.Id);

        if (vehicle == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await ValidateUniqueVehicleFieldsAsync(model, model.Id);

        if (model.CurrentMileageKm < vehicle.CurrentMileageKm)
        {
            ModelState.AddModelError(
                nameof(model.CurrentMileageKm),
                "Километражът не може да бъде по-малък от текущия.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        vehicle.RegistrationNumber = model.RegistrationNumber;
        vehicle.Make = model.Make;
        vehicle.Model = model.Model;
        vehicle.Year = model.Year;
        vehicle.VIN = model.VIN;
        vehicle.FuelType = model.FuelType!.Value;
        vehicle.CurrentMileageKm = model.CurrentMileageKm;
        vehicle.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new VehicleDeleteViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model
            })
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return NotFound();
        }

        return View(vehicle);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        var model = new VehicleDeleteViewModel
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Make = vehicle.Make,
            Model = vehicle.Model
        };

        if (await _vehicleService.HasHistoryAsync(id))
        {
            ModelState.AddModelError(
                string.Empty,
                "Автомобилът има записани зареждания или ремонти и не може да бъде изтрит.");

            return View("Delete", model);
        }

        try
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException { Number: 547 })
        {
            ModelState.AddModelError(
                string.Empty,
                "Автомобилът не може да бъде изтрит, защото има свързани записи.");

            return View("Delete", model);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueVehicleFieldsAsync(
        VehicleFormViewModel model,
        int? excludedVehicleId = null)
    {
        var result = await _vehicleService.CheckDuplicatesAsync(
            model.RegistrationNumber,
            model.VIN,
            excludedVehicleId);

        if (result.RegistrationExists)
        {
            ModelState.AddModelError(
                nameof(model.RegistrationNumber),
                "Вече съществува автомобил с този регистрационен номер.");
        }

        if (result.VinExists)
        {
            ModelState.AddModelError(
                nameof(model.VIN),
                "Вече съществува автомобил с този VIN.");
        }
    }
}