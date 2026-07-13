using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetAllWithCategoryAsync();

    Task<Product?> GetByIdWithCategoryAsync(int id);
}
