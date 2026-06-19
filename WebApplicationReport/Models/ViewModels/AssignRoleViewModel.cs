namespace WebApplicationReport.Models.ViewModels;

public class AssignRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> AllRoles { get; set; } = [];
    public List<string> SelectedRoles { get; set; } = [];
}
