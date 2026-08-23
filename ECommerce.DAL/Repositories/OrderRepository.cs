using ECommerce.DAL.Data;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Repositories;

public class OrderRepository(ApplicationDbContext context) : IOrderRepository
{
    public async Task AddAsync(Order order)
    {
        await context.Orders.AddAsync(order);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(string userId)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAndUserIdAsync(int id, string userId)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id && order.UserId == userId);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        return await context.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id);
    }
}
