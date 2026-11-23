using FleetLog.Data;
using FleetLog.Services;
using FleetLog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Administrator)]
public class VehicleAssignmentsController : Controller
{
    private readonly FleetLogDbContext _context;
    private readonly VehicleAssignmentService _assignmentService;

    public VehicleAssignmentsController(
        FleetLogDbContext context,
        VehicleAssignmentService assignmentService)
    {
        _context = context;
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _context.VehicleAssignments
            .AsNoTracking()
            .OrderByDescending(a => a.StartDate)
            .ThenByDescending(a => a.Id)
            .Select(a => new VehicleAssignmentListViewModel
            {
                Id = a.Id,
                RegistrationNumber = a.Vehicle.RegistrationNumber,
                DriverName = a.Driver.FullName,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new VehicleAssignmentCreateViewModel();
        await PopulateOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        VehicleAssignmentCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _assignmentService.AssignAsync(
                model.VehicleId!.Value,
                model.DriverId!.Value,
                model.StartDate!.Value,
                model.EndDate);

            switch (result)
            {
                case VehicleAssignmentResult.Success:
                    return RedirectToAction(nameof(Index));

                case VehicleAssignmentResult.VehicleNotFound:
                case VehicleAssignmentResult.VehicleInactive:
                    ModelState.AddModelError(
                        nameof(model.VehicleId),
                        "Изберете съществуващ активен автомобил.");
                    break;

                case VehicleAssignmentResult.DriverNotFound:
                case VehicleAssignmentResult.DriverInactive:
                    ModelState.AddModelError(
                        nameof(model.DriverId),
                        "Изберете съществуващ активен шофьор.");
                    break;

                case VehicleAssignmentResult.EndBeforeStart:
                    ModelState.AddModelError(
                        nameof(model.EndDate),
                        "Крайната дата не може да бъде преди началната.");
                    break;

                case VehicleAssignmentResult.OverlappingAssignment:
                    ModelState.AddModelError(
                        string.Empty,
                        "Автомобилът вече е назначен за част от избрания период.");
                    break;

                default:
                    throw new InvalidOperationException(
                        "Unexpected assignment creation result.");
            }
        }

        await PopulateOptionsAsync(model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Release(int id)
    {
        var assignment = await _context.VehicleAssignments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.StartDate,
                a.EndDate,
                RegistrationNumber = a.Vehicle.RegistrationNumber,
                DriverName = a.Driver.FullName
            })
            .SingleOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        if (assignment.EndDate.HasValue)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new VehicleAssignmentReleaseViewModel
        {
            Id = assignment.Id,
            RegistrationNumber = assignment.RegistrationNumber,
            DriverName = assignment.DriverName,
            StartDate = assignment.StartDate,
            EndDate = assignment.StartDate.Date > DateTime.Today
                ? assignment.StartDate.Date
                : DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Release(
        VehicleAssignmentReleaseViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _assignmentService.ReleaseAsync(
                model.Id,
                model.EndDate!.Value);

            switch (result)
            {
                case VehicleAssignmentResult.Success:
                    return RedirectToAction(nameof(Index));

                case VehicleAssignmentResult.AssignmentNotFound:
                    return NotFound();

                case VehicleAssignmentResult.AssignmentAlreadyEnded:
                    ModelState.AddModelError(
                        string.Empty,
                        "За това назначение вече е зададена крайна дата.");
                    break;

                case VehicleAssignmentResult.EndBeforeStart:
                    ModelState.AddModelError(
                        nameof(model.EndDate),
                        "Крайната дата не може да бъде преди началната.");
                    break;

                default:
                    throw new InvalidOperationException(
                        "Unexpected assignment release result.");
            }
        }

        var assignment = await _context.VehicleAssignments
            .AsNoTracking()
            .Where(a => a.Id == model.Id)
            .Select(a => new
            {
                a.StartDate,
                RegistrationNumber = a.Vehicle.RegistrationNumber,
                DriverName = a.Driver.FullName
            })
            .SingleOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        model.RegistrationNumber = assignment.RegistrationNumber;
        model.DriverName = assignment.DriverName;
        model.StartDate = assignment.StartDate;

        return View(model);
    }

    private async Task PopulateOptionsAsync(
        VehicleAssignmentCreateViewModel model)
    {
        model.AvailableVehicles = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.RegistrationNumber)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.RegistrationNumber
            })
            .ToListAsync();

        model.AvailableDrivers = await _context.Drivers
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.FullName)
            .ThenBy(d => d.Id)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.FullName + " (" + (d.User.Email ?? d.UserId) + ")"
            })
            .ToListAsync();
    }
}