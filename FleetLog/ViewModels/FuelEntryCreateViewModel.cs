using System.ComponentModel.DataAnnotations;

namespace FleetLog.ViewModels;

public class FuelEntryCreateViewModel
{
    public int VehicleId { get; set; }
    public string RegistrationNumber { get; set; } =  string.Empty;
    [Required(ErrorMessage = "Изберете дата.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата")]
    public DateTime? Date { get; set; } = DateTime.Today;
    [Required(ErrorMessage = "Въведете количество гориво.")]
    [Range(typeof(decimal), "0.01", "182",
        ErrorMessage = "Количеството трябва да е между 0.01 и 182 литра.")]
    [Display(Name = "Литри")]
    public decimal? Litres { get; set; }
    [Required(ErrorMessage = "Въведете обща сума.")]
    [Range(typeof(decimal), "0", "1000",
        ErrorMessage = "Общата сума трябва да е между 0 и 1000.")]
    [Display(Name = "Обща сума")]
    public decimal? TotalCost { get; set; }
    [Required(ErrorMessage = "Въведете километраж.")]
    [Range(0, int.MaxValue,
        ErrorMessage = "Километражът не може да бъде отрицателен.")]
    [Display(Name = "Километраж (км)")]
    public int? MileageKm { get; set; }
}