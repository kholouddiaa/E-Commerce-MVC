using ECommerce.DAL.Data;
using ECommerce.DAL.Entities;
using ECommerce.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Repositories;

public class GenericRepository<TEntity>(ApplicationDbContext context) : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await DbSet.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        entity.IsDeleted = true;
        DbSet.Update(entity);
    }
}
