namespace ECommerce.Web.ViewModels;

public class UserManagementUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CurrentRole { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public bool IsCustomer { get; set; }

    public bool IsLocked { get; set; }

    public string LockStatus { get; set; } = string.Empty;

    public bool IsCurrentUser { get; set; }
}
