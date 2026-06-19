using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplicationReport.Models.Domain;
using WebApplicationReport.Models.ViewModels;

namespace WebApplicationReport.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ─── Users List ───────────────────────────────────────────────
    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var model = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserRoleViewModel
            {
                UserId    = user.Id,
                Email     = user.Email!,
                Roles     = roles.ToList(),
                IsDeleted = user.IsDeleted
            });
        }

        return View(model);
    }

    // ─── Add User ────────────────────────────────────────────────
    public IActionResult AddUser() => View(new AddUserViewModel
    {
        AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName       = model.Email,
            Email          = model.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.Role))
            await _userManager.AddToRoleAsync(user, model.Role);

        return RedirectToAction(nameof(Users));
    }

    // ─── Edit User ───────────────────────────────────────────────
    public async Task<IActionResult> EditUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var roles    = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        return View(new EditUserViewModel
        {
            UserId   = user.Id,
            Email    = user.Email!,
            AllRoles = allRoles,
            Role     = roles.FirstOrDefault() ?? ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null) return NotFound();

        user.Email               = model.Email;
        user.UserName            = model.Email;
        user.NormalizedEmail     = model.Email.ToUpper();
        user.NormalizedUserName  = model.Email.ToUpper();
        await _userManager.UpdateAsync(user);

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                model.AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
                return View(model);
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!string.IsNullOrEmpty(model.Role))
            await _userManager.AddToRoleAsync(user, model.Role);

        return RedirectToAction(nameof(Users));
    }

    // ─── Assign Role ──────────────────────────────────────────────
    public async Task<IActionResult> AssignRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var allRoles  = _roleManager.Roles.Select(r => r.Name!).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        return View(new AssignRoleViewModel
        {
            UserId        = user.Id,
            Email         = user.Email!,
            AllRoles      = allRoles,
            SelectedRoles = userRoles.ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (model.SelectedRoles?.Count > 0)
            await _userManager.AddToRolesAsync(user, model.SelectedRoles);

        return RedirectToAction(nameof(Users));
    }

    // ─── Soft Delete User ─────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        // Prevent deleting your own account
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Users));
        }

        user.IsDeleted = true;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Users));
    }

    // ─── Restore User ─────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.IsDeleted = false;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Users));
    }
}
