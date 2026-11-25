using System.Security.Claims;
using FleetLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetLog.Controllers;

[Authorize]
public class MyVehiclesController : Controller
{
    private readonly DriverVehicleService _driverVehicleService;

    public MyVehiclesController(
        DriverVehicleService driverVehicleService)
    {
        _driverVehicleService = driverVehicleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var model = await _driverVehicleService.GetCurrentVehiclesAsync(
            userId,
            DateTime.Today);

        return View(model);
    }
}