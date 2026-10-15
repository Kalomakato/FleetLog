using FleetLog.Data;
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
            .Where(v=> v.Id == vehicleId)
            .Select(v=> new VehicleFuelHistoryViewModel
            {
                VehicleId = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                })
            .FirstOrDefaultAsync();


        if (model == null)
        {
            return NotFound();
        }
        
        model.FuelEntries = await  _context.FuelEntries
            .AsNoTracking()
            .Where(f =>f.VehicleId == vehicleId)
            .OrderByDescending(f => f.Date)
            .ThenByDescending(f => f.Id)
            .Select(f => new FuelEntryListViewModel
            {
                Id = f.Id,
                Date = f.Date,
                Litres =  f.Litres,
                TotalCost = f.TotalCost,
                MileageKm = f.MileageKm,
            })
            .ToListAsync();
        
        return View(model);
    }
}