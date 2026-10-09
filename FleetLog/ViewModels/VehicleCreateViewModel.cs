using System.ComponentModel.DataAnnotations;
using FleetLog.Data.Enums;

namespace FleetLog.ViewModels;

public class VehicleCreateViewModel
{
    [Required(ErrorMessage = "Въведете регистрационен номер.")]
    [MaxLength(8, ErrorMessage = "Регистрационният номер трябва да е до 8 символа.")]
    [Display(Name = "Регистрационен номер")]
    public string RegistrationNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Въведете марка.")]
    [MaxLength(50, ErrorMessage = "Марката трябва да е до 50 символа.")]
    [Display(Name = "Марка")]
    public string Make { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете модел.")]
    [MaxLength(100, ErrorMessage = "Моделът трябва да е до 100 символа.")]
    [Display(Name = "Модел")]
    public string Model { get; set; } = string.Empty;
    
    [Range(1900, 2026, ErrorMessage = "Годината трябва да е между 1900 и 2026.")]
    [Display(Name = "Година")]
    public int Year { get; set; }
    
    [Required(ErrorMessage = "Въведете VIN.")]
    [StringLength(17, MinimumLength = 17, ErrorMessage = "VIN трябва да е точно 17 символа.")]
    [Display(Name = "VIN")]
    public string VIN { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Изберете вид гориво.")]
    [EnumDataType(typeof(FuelType), ErrorMessage = "Изберете валиден вид гориво.")]
    [Display(Name = "Гориво")]
    public FuelType? FuelType { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Километражът не може да бъде отрицателен.")]
    [Display(Name = "Километраж (км)")]
    public int CurrentMileageKm { get; set; }
    
    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}