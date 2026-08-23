namespace ECommerce.BLL.DTOs.Reviews;

public class ProductReviewsDto
{
    public int ProductId { get; set; }

    public double? AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<ReviewDto> Reviews { get; set; } = [];

    public ReviewDto? CurrentUserReview { get; set; }
}
