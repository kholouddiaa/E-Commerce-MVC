using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllWithCategoryAsync();

    Task<(IReadOnlyList<Product> Items, int TotalItems, int CurrentPage)> GetPagedWithCategoryAsync(
        string? searchTerm,
        int? categoryId,
        string sortOrder,
        int pageNumber,
        int pageSize);

    Task<Product?> GetByIdWithCategoryAsync(int id);

    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids);

    Task<IReadOnlyList<Product>> GetDeletedWithCategoryAsync();

    Task<Product?> GetDeletedByIdAsync(int id);

    Task<int> GetTotalCountAsync();

    Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync();
}
