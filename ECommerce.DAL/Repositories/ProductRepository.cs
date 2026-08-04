using ECommerce.DAL.Data;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Repositories;

public class ProductRepository(ApplicationDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllWithCategoryAsync()
    {
        return await Context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalItems, int CurrentPage)> GetPagedWithCategoryAsync(
        string? searchTerm,
        int? categoryId,
        string sortOrder,
        int pageNumber,
        int pageSize)
    {
        var query = Context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(product =>
                product.Name.Contains(searchTerm) ||
                (product.Description != null && product.Description.Contains(searchTerm)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        query = sortOrder switch
        {
            "name_desc" => query
                .OrderByDescending(product => product.Name)
                .ThenByDescending(product => product.Id),
            "price_asc" => query
                .OrderBy(product => product.Price)
                .ThenBy(product => product.Name),
            "price_desc" => query
                .OrderByDescending(product => product.Price)
                .ThenBy(product => product.Name),
            _ => query
                .OrderBy(product => product.Name)
                .ThenBy(product => product.Id)
        };

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
        {
            return ([], 0, 1);
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var currentPage = Math.Min(Math.Max(pageNumber, 1), totalPages);

        var items = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems, currentPage);
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await Context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await Context.Products
            .AsNoTracking()
            .CountAsync();
    }

    public async Task<IReadOnlyDictionary<int, int>> GetCategoryProductCountsAsync()
    {
        return await Context.Products
            .AsNoTracking()
            .GroupBy(product => product.CategoryId)
            .ToDictionaryAsync(group => group.Key, group => group.Count());
    }
}
