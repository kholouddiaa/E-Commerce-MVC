using ECommerce.DAL.Data;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Repositories;

public class ReviewRepository(ApplicationDbContext context) : IReviewRepository
{
    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId)
    {
        return await context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .Where(review => review.ProductId == productId)
            .OrderByDescending(review => review.UpdatedAt)
            .ThenByDescending(review => review.Id)
            .ToListAsync();
    }

    public async Task<(double? AverageRating, int ReviewCount)> GetSummaryByProductIdAsync(int productId)
    {
        var summary = await context.Reviews
            .AsNoTracking()
            .Where(review => review.ProductId == productId)
            .GroupBy(review => review.ProductId)
            .Select(group => new
            {
                ReviewCount = group.Count(),
                AverageRating = group.Average(review => (double)review.Rating)
            })
            .FirstOrDefaultAsync();

        return summary is null
            ? (null, 0)
            : (summary.AverageRating, summary.ReviewCount);
    }

    public async Task<Review?> GetByProductIdAndUserIdAsync(int productId, string userId)
    {
        return await context.Reviews
            .AsNoTracking()
            .Include(review => review.User)
            .FirstOrDefaultAsync(review => review.ProductId == productId && review.UserId == userId);
    }

    public async Task<Review?> GetByIdAndUserIdAsync(int id, string userId)
    {
        return await context.Reviews
            .FirstOrDefaultAsync(review => review.Id == id && review.UserId == userId);
    }

    public async Task AddAsync(Review review)
    {
        await context.Reviews.AddAsync(review);
    }

    public void Update(Review review)
    {
        context.Reviews.Update(review);
    }

    public void Delete(Review review)
    {
        context.Reviews.Remove(review);
    }
}
