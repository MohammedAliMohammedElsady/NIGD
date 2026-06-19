using System.ComponentModel.DataAnnotations;

namespace WebApplicationReport.Models.ViewModels;

public class EditUserViewModel
{
    public string UserId { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password), MinLength(6)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string? ConfirmPassword { get; set; }

    public string? Role { get; set; }
    public List<string> AllRoles { get; set; } = [];
}
