using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByProductIdAsync(int productId);

    Task<(double? AverageRating, int ReviewCount)> GetSummaryByProductIdAsync(int productId);

    Task<Review?> GetByProductIdAndUserIdAsync(int productId, string userId);

    Task<Review?> GetByIdAndUserIdAsync(int id, string userId);

    Task AddAsync(Review review);

    void Update(Review review);

    void Delete(Review review);
}
