using ECommerce.DAL.Data;
using ECommerce.DAL.Repositories;
using ECommerce.DAL.Repositories.Interfaces;
using ECommerce.DAL.UnitOfWork.Interfaces;

namespace ECommerce.DAL.UnitOfWork;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IProductRepository? _products;
    private ICategoryRepository? _categories;

    public IProductRepository Products => _products ??= new ProductRepository(context);

    public ICategoryRepository Categories => _categories ??= new CategoryRepository(context);

    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
    {
        return context.DisposeAsync();
    }
}
