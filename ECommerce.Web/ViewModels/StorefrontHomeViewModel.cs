using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class StorefrontHomeViewModel
{
    public IReadOnlyList<StoreCategoryViewModel> Categories { get; set; } = [];

    public IReadOnlyList<ProductDto> FeaturedProducts { get; set; } = [];

    public IReadOnlyList<ProductDto> NewArrivals { get; set; } = [];
}
