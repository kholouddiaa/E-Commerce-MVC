namespace ECommerce.BLL.DTOs.Products;

public class ProductQueryParameters
{
    public const int DefaultPageSize = 9;
    public const int MaxPageSize = 24;

    public string? SearchTerm { get; set; }

    public string SortOrder { get; set; } = ProductSortOrder.NameAscending;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;

    public int? CategoryId { get; set; }
}
