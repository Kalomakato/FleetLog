using FleetLog.Data;
using FleetLog.Data.Models;
using FleetLog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Controllers;

public class RepairsController : Controller
{
    private readonly FleetLogDbContext _context;

    public RepairsController(FleetLogDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<IActionResult> Index(int vehicleId)
    {
        var model = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == vehicleId)
            .Select(v => new VehicleRepairHistoryViewModel
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber
            })
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return NotFound();
        }

        model.Repairs = await _context.Repairs
            .AsNoTracking()
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Id)
            .Select(r => new RepairListViewModel
            {
                Id = r.Id,
                Date = r.Date,
                Description = r.Description,
                TotalCost = r.TotalCost,
                MileageKm = r.MileageKm,
                ServiceCentre = r.ServiceCentre
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
            .Select(v => new RepairCreateViewModel
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                MileageKm = v.CurrentMileageKm
            })
            .FirstOrDefaultAsync();
        if (model==null)
        {
            return NotFound();
        }
        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RepairCreateViewModel model)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == model.VehicleId);

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

        var repair = new Repair
        {
            VehicleId = vehicle.Id,
            Date = model.Date!.Value.Date,
            Description = model.Description.Trim(),
            TotalCost = model.TotalCost!.Value,
            MileageKm = model.MileageKm!.Value,
            ServiceCentre = string.IsNullOrWhiteSpace(model.ServiceCentre)
                ? null
                : model.ServiceCentre.Trim()
        };

        _context.Repairs.Add(repair);
        vehicle.CurrentMileageKm = repair.MileageKm;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { vehicleId = vehicle.Id });
    }
}