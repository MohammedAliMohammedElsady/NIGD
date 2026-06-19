using Microsoft.AspNetCore.Identity;

namespace WebApplicationReport.Models.Domain;

public class ApplicationUser : IdentityUser
{
    public bool IsDeleted { get; set; } = false;
}
