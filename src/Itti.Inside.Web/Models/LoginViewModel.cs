using System.ComponentModel.DataAnnotations;

namespace Itti.Inside.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Enter your User ID.")]
    [StringLength(256)]
    [Display(Name = "User ID")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your Password.")]
    [StringLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
