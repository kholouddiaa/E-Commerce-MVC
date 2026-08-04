namespace ECommerce.Web.ViewModels;

public class StoreCategoryViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int ProductCount { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}
