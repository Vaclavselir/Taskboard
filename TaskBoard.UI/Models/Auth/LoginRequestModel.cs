using System.ComponentModel.DataAnnotations;

namespace TaskBoard.UI.Models;

public class LoginRequestModel
{

    [Required(ErrorMessage = "Email je povinný.")]
    [EmailAddress(ErrorMessage = "Email nemá správný formát.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Heslo je povinné.")]
    public string Password { get; set; } = string.Empty;

}
