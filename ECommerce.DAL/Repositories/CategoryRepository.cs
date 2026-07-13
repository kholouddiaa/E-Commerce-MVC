using ECommerce.DAL.Data;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Repositories;

public class CategoryRepository(ApplicationDbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    public override async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await Context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var trimmedName = name.Trim();

        return await Context.Categories.AnyAsync(category =>
            category.Name == trimmedName &&
            (!excludeId.HasValue || category.Id != excludeId.Value));
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await Context.Products.AnyAsync(product => product.CategoryId == id);
    }
}
