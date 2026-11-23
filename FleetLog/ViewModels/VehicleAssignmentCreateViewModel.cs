using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetLog.ViewModels;

public class VehicleAssignmentCreateViewModel
{
    [Required(ErrorMessage = "Изберете автомобил.")]
    [Range(1, int.MaxValue, ErrorMessage = "Изберете валиден автомобил.")]
    [Display(Name = "Автомобил")]
    public int? VehicleId { get; set; }

    [Required(ErrorMessage = "Изберете шофьор.")]
    [Range(1, int.MaxValue, ErrorMessage = "Изберете валиден шофьор.")]
    [Display(Name = "Шофьор")]
    public int? DriverId { get; set; }

    [Required(ErrorMessage = "Изберете начална дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Начална дата")]
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Крайна дата (по избор)")]
    public DateTime? EndDate { get; set; }

    [ValidateNever]
    public List<SelectListItem> AvailableVehicles { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> AvailableDrivers { get; set; } = new();
}