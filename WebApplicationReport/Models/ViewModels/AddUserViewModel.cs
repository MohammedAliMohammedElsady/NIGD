using System.ComponentModel.DataAnnotations;

namespace WebApplicationReport.Models.ViewModels;

public class AddUserViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Role { get; set; }
    public List<string> AllRoles { get; set; } = [];
}
