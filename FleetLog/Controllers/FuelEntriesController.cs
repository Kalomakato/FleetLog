using FleetLog.Data;
using FleetLog.ViewModels;
using FleetLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Controllers;

[Authorize]
public class FuelEntriesController : Controller
{
    private readonly FleetLogDbContext _context;
    private readonly FuelEntryService _fuelEntryService;
    private readonly VehicleAccessService _vehicleAccessService;

    public FuelEntriesController(
        FleetLogDbContext context,
        FuelEntryService fuelEntryService,
        VehicleAccessService vehicleAccessService)
    {
        _context = context;
        _fuelEntryService = fuelEntryService;
        _vehicleAccessService = vehicleAccessService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int vehicleId)
    {
        if (!await _vehicleAccessService.CanAccessAsync(User, vehicleId))
        {
            return NotFound();
        }

        var model = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == vehicleId)
            .Select(v => new VehicleFuelHistoryViewModel
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber
            })
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return NotFound();
        }

        model.FuelEntries = await _context.FuelEntries
            .AsNoTracking()
            .Where(f => f.VehicleId == vehicleId)
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Id)
            .Select(f => new FuelEntryListViewModel
            {
                Id = f.Id,
                Date = f.Date,
                Litres = f.Litres,
                TotalCost = f.TotalCost,
                MileageKm = f.MileageKm
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int vehicleId)
    {
        if (!await _vehicleAccessService.CanAccessAsync(User, vehicleId))
        {
            return NotFound();
        }

        var model = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == vehicleId)
            .Select(v => new FuelEntryCreateViewModel
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                MileageKm = v.CurrentMileageKm
            })
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FuelEntryCreateViewModel model)
    {
        if (!await _vehicleAccessService.CanAccessAsync(User, model.VehicleId))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _fuelEntryService.CreateAsync(
                vehicleId: model.VehicleId,
                date: model.Date!.Value,
                litres: model.Litres!.Value,
                totalCost: model.TotalCost!.Value,
                mileageKm: model.MileageKm!.Value);

            if (!result.VehicleFound)
            {
                return NotFound();
            }

            if (result.FutureDate)
            {
                ModelState.AddModelError(
                    nameof(model.Date),
                    "Датата не може да бъде в бъдещето.");
            }

            if (result.MileageTooLow)
            {
                ModelState.AddModelError(
                    nameof(model.MileageKm),
                    "Километражът не може да бъде по-малък от текущия.");
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(Index),
                    new { vehicleId = model.VehicleId });
            }
        }

        var registrationNumber = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == model.VehicleId)
            .Select(v => v.RegistrationNumber)
            .FirstOrDefaultAsync();

        if (registrationNumber is null)
        {
            return NotFound();
        }

        model.RegistrationNumber = registrationNumber;
        return View(model);
    }
}