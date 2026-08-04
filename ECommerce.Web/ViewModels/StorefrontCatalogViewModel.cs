using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class StorefrontCatalogViewModel
{
    public IReadOnlyList<ProductDto> Products { get; set; } = [];

    public IReadOnlyList<StoreCategoryViewModel> Categories { get; set; } = [];

    public string SearchTerm { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public string SelectedCategoryName { get; set; } = "All Categories";

    public bool HasFilters => !string.IsNullOrWhiteSpace(SearchTerm) || CategoryId.HasValue;
}
