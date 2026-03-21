using System.ComponentModel.DataAnnotations;

namespace TaskBoard.UI.Models.Auth;

public sealed class RegisterModel
{

    [Required(ErrorMessage = "Email je povinný.")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Heslo je povinné.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Potvrzení hesla je povinné.")]
    [Compare(nameof(Password), ErrorMessage = "Hesla se neshodují.")]
    public string ConfirmPassword { get; set; } = string.Empty;

}
