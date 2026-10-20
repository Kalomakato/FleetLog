using FleetLog.Data;
using FleetLog.Data.Models;
using FleetLog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Controllers;

public class FuelEntriesController : Controller
{
    private readonly FleetLogDbContext _context;

    public FuelEntriesController(FleetLogDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int vehicleId)
    {
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
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v =>v.Id == model.VehicleId);

        if (vehicle == null)
        {
            return NotFound();
        }

        model.RegistrationNumber = vehicle.RegistrationNumber;

        if (model.Date.HasValue && model.Date.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.Date),
                "Датата не може да бъде в бъдещето.");
        }
        if (model.MileageKm.HasValue &&
            model.MileageKm.Value < vehicle.CurrentMileageKm)
        {
            ModelState.AddModelError(
                nameof(model.MileageKm),
                "Километражът не може да бъде по-малък от текущия.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entry = new FuelEntry
        {
            VehicleId = vehicle.Id,
            Date = model.Date!.Value.Date,
            Litres = model.Litres!.Value,
            TotalCost = model.TotalCost!.Value,
            MileageKm = model.MileageKm!.Value
        };

        _context.FuelEntries.Add(entry);
        vehicle.CurrentMileageKm = entry.MileageKm;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { vehicleId = vehicle.Id });
    }
}
