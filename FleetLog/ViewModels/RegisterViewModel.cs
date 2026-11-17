using System.ComponentModel.DataAnnotations;

namespace FleetLog.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Въведете имейл адрес.")]
    [EmailAddress(ErrorMessage = "Въведете валиден имейл адрес.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Въведете парола.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Паролата трябва да е между 8 и 100 символа.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Потвърдете паролата.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password),
        ErrorMessage = "Паролите не съвпадат.")]
    [Display(Name = "Потвърдете паролата")]
    public string ConfirmPassword { get; set; } = string.Empty;
}