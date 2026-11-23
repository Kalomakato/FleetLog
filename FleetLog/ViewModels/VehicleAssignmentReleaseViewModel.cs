using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FleetLog.ViewModels;

public class VehicleAssignmentReleaseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Изберете крайна дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Крайна дата")]
    public DateTime? EndDate { get; set; } = DateTime.Today;

    [ValidateNever]
    public string RegistrationNumber { get; set; } = string.Empty;

    [ValidateNever]
    public string DriverName { get; set; } = string.Empty;

    [ValidateNever]
    public DateTime StartDate { get; set; }
}