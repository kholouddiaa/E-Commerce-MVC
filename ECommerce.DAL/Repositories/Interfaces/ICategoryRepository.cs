using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);

    Task<bool> HasProductsAsync(int id);
}
