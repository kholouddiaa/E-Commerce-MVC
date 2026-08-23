using ECommerce.BLL.Common;
using ECommerce.BLL.DTOs.Reviews;
using ECommerce.BLL.Services.Interfaces;
using ECommerce.DAL.Entities;
using ECommerce.DAL.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.BLL.Services;

public class ReviewService(IUnitOfWork unitOfWork) : IReviewService
{
    public async Task<ProductReviewsDto> GetProductReviewsAsync(int productId, string? currentUserId)
    {
        var (averageRating, reviewCount) = await unitOfWork.Reviews.GetSummaryByProductIdAsync(productId);
        var reviews = await unitOfWork.Reviews.GetByProductIdAsync(productId);

        var reviewDtos = reviews
            .Select(MapReview)
            .ToList();

        return new ProductReviewsDto
        {
            ProductId = productId,
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            Reviews = reviewDtos,
            CurrentUserReview = string.IsNullOrWhiteSpace(currentUserId)
                ? null
                : reviewDtos.FirstOrDefault(review => review.UserId == currentUserId)
        };
    }

    public async Task<OperationResult> CreateAsync(string userId, ReviewUpsertDto reviewDto)
    {
        var normalizedComment = NormalizeComment(reviewDto.Comment);
        if (normalizedComment is null)
        {
            return OperationResult.Failure("Review text is required.", nameof(ReviewUpsertDto.Comment));
        }

        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            return OperationResult.Failure("Rating must be between 1 and 5.", nameof(ReviewUpsertDto.Rating));
        }

        var product = await unitOfWork.Products.GetByIdAsync(reviewDto.ProductId);
        if (product is null)
        {
            return OperationResult.Failure("The requested product was not found.");
        }

        var existingReview = await unitOfWork.Reviews.GetByProductIdAndUserIdAsync(reviewDto.ProductId, userId);
        if (existingReview is not null)
        {
            return OperationResult.Failure("You have already reviewed this product.");
        }

        var review = new Review
        {
            ProductId = reviewDto.ProductId,
            UserId = userId,
            Rating = reviewDto.Rating,
            Comment = normalizedComment
        };

        try
        {
            await unitOfWork.Reviews.AddAsync(review);
            await unitOfWork.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure("You have already reviewed this product.");
        }
        catch
        {
            return OperationResult.Failure("Unable to save your review.");
        }
    }

    public async Task<OperationResult> UpdateAsync(int id, string userId, ReviewUpsertDto reviewDto)
    {
        var normalizedComment = NormalizeComment(reviewDto.Comment);
        if (normalizedComment is null)
        {
            return OperationResult.Failure("Review text is required.", nameof(ReviewUpsertDto.Comment));
        }

        if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
        {
            return OperationResult.Failure("Rating must be between 1 and 5.", nameof(ReviewUpsertDto.Rating));
        }

        var review = await unitOfWork.Reviews.GetByIdAndUserIdAsync(id, userId);
        if (review is null || review.ProductId != reviewDto.ProductId)
        {
            return OperationResult.Failure("The review was not found.");
        }

        var product = await unitOfWork.Products.GetByIdAsync(review.ProductId);
        if (product is null)
        {
            return OperationResult.Failure("The requested product was not found.");
        }

        review.Rating = reviewDto.Rating;
        review.Comment = normalizedComment;

        try
        {
            unitOfWork.Reviews.Update(review);
            await unitOfWork.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch
        {
            return OperationResult.Failure("Unable to update your review.");
        }
    }

    public async Task<OperationResult> DeleteAsync(int id, string userId)
    {
        var review = await unitOfWork.Reviews.GetByIdAndUserIdAsync(id, userId);
        if (review is null)
        {
            return OperationResult.Failure("The review was not found.");
        }

        try
        {
            unitOfWork.Reviews.Delete(review);
            await unitOfWork.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch
        {
            return OperationResult.Failure("Unable to delete your review.");
        }
    }

    private static ReviewDto MapReview(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            Rating = review.Rating,
            Comment = review.Comment,
            ReviewerName = GetReviewerName(review),
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    private static string GetReviewerName(Review review)
    {
        if (!string.IsNullOrWhiteSpace(review.User?.FullName))
        {
            return review.User.FullName;
        }

        if (!string.IsNullOrWhiteSpace(review.User?.UserName))
        {
            return review.User.UserName;
        }

        return "Customer";
    }

    private static string? NormalizeComment(string? comment)
    {
        return string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
