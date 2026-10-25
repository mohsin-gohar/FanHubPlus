using FanHubPlus.Core.Interfaces;
using FanHubPlus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Infrastructure.Repositories;

/// <summary>
/// Generic EF Core backed repository.
/// </summary>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => QueryNoTracking();

    public IQueryable<TEntity> QueryNoTracking() => DbSet.AsQueryable();

    public IQueryable<TEntity> QueryTracked() => DbSet;

    public Task<TEntity?> GetByIdAsync(object id) => DbSet.FindAsync(id).AsTask();

    public async Task<List<TEntity>> ListAsync() => await DbSet.AsNoTracking().ToListAsync();

    public void Add(TEntity entity) => DbSet.Add(entity);

    public Task AddAsync(TEntity entity) => DbSet.AddAsync(entity, CancellationToken.None).AsTask();

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => DbContext.SaveChangesAsync(ct);
}