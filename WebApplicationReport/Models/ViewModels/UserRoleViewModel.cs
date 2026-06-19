namespace WebApplicationReport.Models.ViewModels;

public class UserRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public bool IsDeleted { get; set; }
}
