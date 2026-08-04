using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class StorefrontCatalogViewModel
{
    public IReadOnlyList<ProductDto> Products { get; set; } = [];

    public IReadOnlyList<StoreCategoryViewModel> Categories { get; set; } = [];

    public string SearchTerm { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public string SortOrder { get; set; } = ProductSortOrder.NameAscending;

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = ProductQueryParameters.DefaultPageSize;

    public int TotalItems { get; set; }

    public int TotalPages { get; set; } = 1;

    public string SelectedCategoryName { get; set; } = "All Categories";

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(SearchTerm) ||
        CategoryId.HasValue ||
        SortOrder != ProductSortOrder.NameAscending;
}
