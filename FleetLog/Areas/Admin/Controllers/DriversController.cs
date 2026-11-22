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
public class DriversController : Controller
{
    private readonly FleetLogDbContext _context;
    private readonly DriverService _driverService;

    public DriversController(
        FleetLogDbContext context,
        DriverService driverService)
    {
        _context = context;
        _driverService = driverService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await _context.Drivers
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ThenBy(d => d.Id)
            .Select(d => new DriverListViewModel
            {
                Id = d.Id,
                FullName = d.FullName,
                Email = d.User.Email ?? string.Empty,
                IsActive = d.IsActive
            })
            .ToListAsync();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new DriverCreateViewModel();
        await PopulateAvailableUsersAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DriverCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _driverService.CreateAsync(
                model.FullName,
                model.UserId);

            switch (result)
            {
                case DriverCreateResult.Success:
                    return RedirectToAction(nameof(Index));

                case DriverCreateResult.InvalidName:
                    ModelState.AddModelError(
                        nameof(model.FullName),
                        "Въведете име между 1 и 100 символа.");
                    break;

                case DriverCreateResult.AccountNotFound:
                    ModelState.AddModelError(
                        nameof(model.UserId),
                        "Избраният потребителски профил не съществува.");
                    break;

                case DriverCreateResult.AccountAlreadyLinked:
                    ModelState.AddModelError(
                        nameof(model.UserId),
                        "Този профил вече е свързан с шофьор.");
                    break;

                default:
                    throw new InvalidOperationException(
                        "Unexpected driver creation result.");
            }
        }

        await PopulateAvailableUsersAsync(model);
        return View(model);
    }

    private async Task PopulateAvailableUsersAsync(
        DriverCreateViewModel model)
    {
        model.AvailableUsers = await _context.Users
            .AsNoTracking()
            .Where(u => !_context.Drivers.Any(d => d.UserId == u.Id))
            .OrderBy(u => u.Email)
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.Email ?? u.UserName ?? u.Id
            })
            .ToListAsync();
    }
}