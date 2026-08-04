namespace ECommerce.BLL.DTOs.Products;

public static class ProductSortOrder
{
    public const string NameAscending = "name_asc";
    public const string NameDescending = "name_desc";
    public const string PriceAscending = "price_asc";
    public const string PriceDescending = "price_desc";

    public static string Normalize(string? sortOrder)
    {
        return sortOrder switch
        {
            NameDescending => NameDescending,
            PriceAscending => PriceAscending,
            PriceDescending => PriceDescending,
            _ => NameAscending
        };
    }
}
