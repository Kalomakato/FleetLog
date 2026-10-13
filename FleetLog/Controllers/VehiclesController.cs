using FleetLog.ViewModels;
using Microsoft.EntityFrameworkCore;
using FleetLog.Data;
using FleetLog.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
    // "model" clashes with the car's "Model" field.
    // Empty prefix fixes the form values coming through empty.
    // "Model" is the car model, like Corolla or Cayenne.
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v=> new VehicleEditViewModel
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
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "")] VehicleEditViewModel model)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == model.Id);

        if (vehicle == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _context.Vehicles.AnyAsync(
                v => v.Id != model.Id &&
                     v.RegistrationNumber == model.RegistrationNumber))
        {
            ModelState.AddModelError(
                nameof(model.RegistrationNumber),
                "Вече съществува автомобил с този регистрационен номер.");
        }

        if (await _context.Vehicles.AnyAsync(
                v => v.Id != model.Id && v.VIN == model.VIN))
        {
            ModelState.AddModelError(
                nameof(model.VIN),
                "Вече съществува автомобил с този VIN.");
        }
        
        if (model.CurrentMileageKm < vehicle.CurrentMileageKm)
        {
            ModelState.AddModelError(
                nameof(model.CurrentMileageKm),
                "Километражът не може да бъде по-малък от текущия.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        vehicle.RegistrationNumber = model.RegistrationNumber;
        vehicle.Make = model.Make;
        vehicle.Model = model.Model;
        vehicle.Year = model.Year;
        vehicle.VIN = model.VIN;
        vehicle.FuelType = model.FuelType!.Value;
        vehicle.CurrentMileageKm = model.CurrentMileageKm;
        vehicle.IsActive = model.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .Where(v=> v.Id == id)
            .Select(v => new VehicleDeleteViewModel
            {
                Id = v.Id,
                RegistrationNumber =  v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                })
            .FirstOrDefaultAsync();

        if (vehicle == null)
        {
            return NotFound();
        }
        
        return View(vehicle);
            
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }
        
        var hasFuelEntries = await _context.FuelEntries
            .AnyAsync(f => f.VehicleId == id);
        
        var hasRepairs = await  _context.Repairs
            .AnyAsync(r => r.VehicleId == id);

        if (hasFuelEntries || hasRepairs)
        {
            ModelState.AddModelError(
                string.Empty,
                "Автомобилът има записани зареждания или ремонти и не може да бъде изтрит.");

            var model = new VehicleDeleteViewModel
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
            };
            return View("Delete", model);
        }

        try
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException { Number: 547 })
        {
            ModelState.AddModelError(
                string.Empty,
                "Автомобилът не може да бъде изтрит, защото има свързани записи.");

            var model = new VehicleDeleteViewModel
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                Make = vehicle.Make,
                Model = vehicle.Model
            };

            return View("Delete", model);
        }
        return RedirectToAction(nameof(Index));
    }
    
}