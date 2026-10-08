using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;
using FleetLog.Data;
using Microsoft.AspNetCore.Mvc;

namespace FleetLog.Controllers;

public class VehiclesController : Controller
{
    
    
    private readonly FleetLogDbContext _context;
    
    public VehiclesController(FleetLogDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
       
        var vehicles = await _context.Vehicles
            .AsNoTracking()
            .OrderBy(v => v.RegistrationNumber)
            .Select(v => new VehicleListViewModel
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                CurrentMileageKm = v.CurrentMileageKm,
                IsActive = v.IsActive
            })
            .ToListAsync(); 
        
        
        
        return View(vehicles);
    }
    
    public async Task<IActionResult> Details(int id)
    {
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
}