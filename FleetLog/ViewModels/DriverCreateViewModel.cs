using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FleetLog.ViewModels;

public class DriverCreateViewModel
{
    [Required(ErrorMessage = "Въведете име на шофьора.")]
    [StringLength(100, ErrorMessage = "Името може да съдържа до 100 символа.")]
    [Display(Name = "Име на шофьора")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Изберете потребителски профил.")]
    [Display(Name = "Потребителски профил")]
    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public List<SelectListItem> AvailableUsers { get; set; } = new();
}