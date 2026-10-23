using System.ComponentModel.DataAnnotations;

namespace FleetLog.ViewModels;

public class RepairCreateViewModel
{
    public int VehicleId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Изберете дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата")]
    public DateTime? Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Въведете описание на ремонта.")]
    [MaxLength(1000, ErrorMessage = "Описанието трябва да е до 1000 символа.")]
    [Display(Name = "Описание на ремонта")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете обща сума.")]
    [Range(typeof(decimal), "0", "100000",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Общата сума трябва да е между 0 и 100000.")]
    [Display(Name = "Обща сума")]
    public decimal? TotalCost { get; set; }

    [Required(ErrorMessage = "Въведете километраж.")]
    [Range(0, int.MaxValue,
        ErrorMessage = "Километражът не може да бъде отрицателен.")]
    [Display(Name = "Километраж (км)")]
    public int? MileageKm { get; set; }

    [MaxLength(200, ErrorMessage = "Името на сервиза трябва да е до 200 символа.")]
    [Display(Name = "Сервиз")]
    public string? ServiceCentre { get; set; }
}