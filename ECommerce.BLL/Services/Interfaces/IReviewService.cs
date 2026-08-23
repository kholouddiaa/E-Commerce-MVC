using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Reviews;

namespace ECommerce.BLL.Services.Interfaces;

public interface IReviewService
{
    Task<ProductReviewsDto> GetProductReviewsAsync(int productId, string? currentUserId);

    Task<OperationResult> CreateAsync(string userId, ReviewUpsertDto reviewDto);

    Task<OperationResult> UpdateAsync(int id, string userId, ReviewUpsertDto reviewDto);

    Task<OperationResult> DeleteAsync(int id, string userId);
}
