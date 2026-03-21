using System.ComponentModel.DataAnnotations;
namespace TaskBoard.UI.Models.Auth;

public sealed class LoginModel
{

    [Required(ErrorMessage = "Email je povinný.")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Heslo je povinné.")]
    public string Password { get; set; } = string.Empty;


}
