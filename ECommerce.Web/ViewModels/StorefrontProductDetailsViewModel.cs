using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class StorefrontProductDetailsViewModel
{
    public ProductDto Product { get; set; } = new();

    public IReadOnlyList<ProductDto> RelatedProducts { get; set; } = [];
}
