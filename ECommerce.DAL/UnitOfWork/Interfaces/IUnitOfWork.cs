using ECommerce.DAL.Repositories.Interfaces;

namespace ECommerce.DAL.UnitOfWork.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IProductRepository Products { get; }

    ICategoryRepository Categories { get; }

    IOrderRepository Orders { get; }

    IReviewRepository Reviews { get; }

    Task<int> SaveChangesAsync();
}
