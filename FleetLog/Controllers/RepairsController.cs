using FleetLog.Data;
using FleetLog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetLog.Services;

namespace FleetLog.Controllers;

public class RepairsController : Controller
{
    private readonly FleetLogDbContext _context;
    private readonly RepairService _repairService;

    public RepairsController(
        FleetLogDbContext context,
        RepairService repairService)
    {
        _context = context;
        _repairService = repairService;
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
        if (ModelState.IsValid)
        {
            var result = await _repairService.CreateAsync(
                vehicleId: model.VehicleId,
                date: model.Date!.Value,
                description: model.Description,
                totalCost: model.TotalCost!.Value,
                mileageKm: model.MileageKm!.Value,
                serviceCentre: model.ServiceCentre);

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