using ECommerce.BLL.DTOs.Reviews;
using ECommerce.BLL.DTOs.Products;

namespace ECommerce.Web.ViewModels;

public class StorefrontProductDetailsViewModel
{
    public ProductDto Product { get; set; } = new();

    public IReadOnlyList<ProductDto> RelatedProducts { get; set; } = [];

    public double? AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<ReviewDto> Reviews { get; set; } = [];

    public ReviewDto? CurrentUserReview { get; set; }

    public ReviewUpsertDto ReviewForm { get; set; } = new();
}
