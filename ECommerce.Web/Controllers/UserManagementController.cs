using ECommerce.DAL.Entities;
using ECommerce.Web.Helpers;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UserManagementController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private const string AdminRoleName = "Admin";
    private const string CustomerRoleName = "Customer";
    private const int PageSize = 10;

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        var model = await BuildIndexViewModelAsync(searchTerm, page);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(string id, string? searchTerm = null, int page = 1)
    {
        return await ChangeRoleAsync(id, AdminRoleName, "User promoted to Admin successfully.", searchTerm, page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoteToCustomer(string id, string? searchTerm = null, int page = 1)
    {
        return await ChangeRoleAsync(id, CustomerRoleName, "User changed to Customer successfully.", searchTerm, page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id, string? searchTerm = null, int page = 1)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user was not found.";
            return RedirectToIndex(searchTerm, page);
        }

        if (user.Id == userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot lock your own administrator account.";
            return RedirectToIndex(searchTerm, page);
        }

        var enableLockoutResult = await userManager.SetLockoutEnabledAsync(user, true);
        if (!enableLockoutResult.Succeeded)
        {
            SetIdentityErrorMessage(enableLockoutResult, "Unable to enable lockout for the selected user.");
            return RedirectToIndex(searchTerm, page);
        }

        var lockResult = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        if (!lockResult.Succeeded)
        {
            SetIdentityErrorMessage(lockResult, "Unable to lock the selected user.");
            return RedirectToIndex(searchTerm, page);
        }

        TempData["SuccessMessage"] = $"User '{user.UserName}' has been locked.";
        return RedirectToIndex(searchTerm, page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id, string? searchTerm = null, int page = 1)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user was not found.";
            return RedirectToIndex(searchTerm, page);
        }

        var unlockResult = await userManager.SetLockoutEndDateAsync(user, null);
        if (!unlockResult.Succeeded)
        {
            SetIdentityErrorMessage(unlockResult, "Unable to unlock the selected user.");
            return RedirectToIndex(searchTerm, page);
        }

        TempData["SuccessMessage"] = $"User '{user.UserName}' has been unlocked.";
        return RedirectToIndex(searchTerm, page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string? searchTerm = null, int page = 1)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user was not found.";
            return RedirectToIndex(searchTerm, page);
        }

        if (user.Id == userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot delete your own administrator account.";
            return RedirectToIndex(searchTerm, page);
        }

        var deleteResult = await userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            SetIdentityErrorMessage(deleteResult, "Unable to delete the selected user.");
            return RedirectToIndex(searchTerm, page);
        }

        TempData["SuccessMessage"] = $"User '{user.UserName}' has been deleted.";
        return RedirectToIndex(searchTerm, page);
    }

    private async Task<UserManagementIndexViewModel> BuildIndexViewModelAsync(string? searchTerm, int page)
    {
        var normalizedSearchTerm = searchTerm?.Trim();
        var currentPage = Math.Max(1, page);
        var currentUserId = userManager.GetUserId(User);
        var query = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(user =>
                (user.UserName != null && user.UserName.Contains(normalizedSearchTerm)) ||
                (user.Email != null && user.Email.Contains(normalizedSearchTerm)) ||
                user.FullName.Contains(normalizedSearchTerm));
        }

        var totalUsers = await query.CountAsync();
        var totalPages = totalUsers == 0
            ? 1
            : (int)Math.Ceiling(totalUsers / (double)PageSize);

        if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        var users = await query
            .OrderBy(user => user.UserName)
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var userRows = new List<UserManagementUserViewModel>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var isLocked = user.LockoutEnabled &&
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            userRows.Add(new UserManagementUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "No username",
                Email = user.Email ?? "No email",
                CurrentRole = roles.Count > 0 ? string.Join(", ", roles.OrderBy(role => role)) : "No role assigned",
                IsAdmin = roles.Contains(AdminRoleName),
                IsCustomer = roles.Contains(CustomerRoleName),
                IsLocked = isLocked,
                LockStatus = isLocked
                    ? $"Locked until {user.LockoutEnd!.Value.LocalDateTime:g}"
                    : "Active",
                IsCurrentUser = user.Id == currentUserId
            });
        }

        return new UserManagementIndexViewModel
        {
            Users = userRows,
            SearchTerm = normalizedSearchTerm ?? string.Empty,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalUsers = totalUsers
        };
    }

    private async Task<IActionResult> ChangeRoleAsync(
        string id,
        string targetRole,
        string successMessage,
        string? searchTerm,
        int page)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user was not found.";
            return RedirectToIndex(searchTerm, page);
        }

        if (targetRole == CustomerRoleName && user.Id == userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot remove your own Admin role.";
            return RedirectToIndex(searchTerm, page);
        }

        if (!await roleManager.RoleExistsAsync(targetRole))
        {
            TempData["ErrorMessage"] = $"The '{targetRole}' role is not available.";
            return RedirectToIndex(searchTerm, page);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var managedRolesToRemove = currentRoles
            .Where(role => (role == AdminRoleName || role == CustomerRoleName) && role != targetRole)
            .ToList();

        if (managedRolesToRemove.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, managedRolesToRemove);
            if (!removeResult.Succeeded)
            {
                SetIdentityErrorMessage(removeResult, "Unable to update the selected user's role.");
                return RedirectToIndex(searchTerm, page);
            }
        }

        if (!currentRoles.Contains(targetRole))
        {
            var addResult = await userManager.AddToRoleAsync(user, targetRole);
            if (!addResult.Succeeded)
            {
                if (managedRolesToRemove.Count > 0)
                {
                    await userManager.AddToRolesAsync(user, managedRolesToRemove);
                }

                SetIdentityErrorMessage(addResult, "Unable to update the selected user's role.");
                return RedirectToIndex(searchTerm, page);
            }
        }

        TempData["SuccessMessage"] = successMessage;
        return RedirectToIndex(searchTerm, page);
    }

    private IActionResult RedirectToIndex(string? searchTerm, int page)
    {
        return RedirectToAction(nameof(Index), new
        {
            searchTerm,
            page = Math.Max(1, page)
        });
    }

    private void SetIdentityErrorMessage(IdentityResult result, string fallbackMessage)
    {
        var errors = result.Errors.Select(error => error.Description).ToList();
        TempData["ErrorMessage"] = errors.Count > 0
            ? string.Join(" ", errors)
            : fallbackMessage;
    }
}
