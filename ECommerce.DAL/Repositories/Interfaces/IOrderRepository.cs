using ECommerce.DAL.Entities;

namespace ECommerce.DAL.Repositories.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId);

    Task<Order?> GetByIdAndUserIdAsync(int id, string userId);

    Task<IReadOnlyList<Order>> GetAllAsync();

    Task<Order?> GetByIdAsync(int id);
}
