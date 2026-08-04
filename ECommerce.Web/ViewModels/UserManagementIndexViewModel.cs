namespace ECommerce.Web.ViewModels;

public class UserManagementIndexViewModel
{
    public IReadOnlyList<UserManagementUserViewModel> Users { get; set; } = [];

    public string SearchTerm { get; set; } = string.Empty;

    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; } = 1;

    public int TotalUsers { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;
}
