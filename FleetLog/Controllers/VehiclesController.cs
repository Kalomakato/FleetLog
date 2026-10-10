using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;
using FleetLog.Data;
using FleetLog.Data.Models;
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
    [HttpGet]
    public IActionResult Create()
    {
        return View(new VehicleCreateViewModel());
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "")] VehicleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        if (await _context.Vehicles.AnyAsync(
                v => v.RegistrationNumber == model.RegistrationNumber))
        {
            ModelState.AddModelError(
                nameof(model.RegistrationNumber),
                "Вече съществува автомобил с този регистрационен номер.");
        }
        
        if (await _context.Vehicles.AnyAsync(v => v.VIN == model.VIN))
        {
            ModelState.AddModelError(
                nameof(model.VIN),
                "Вече съществува автомобил с този VIN-Рама.");
        }
        
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
}