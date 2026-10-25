using System.Linq;

namespace FanHubPlus.Core.Interfaces;

/// <summary>
/// Generic repository interface for all entities.
/// Provides standard CRUD operations with async support.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    // Compose LINQ (Include / Where / OrderBy) without exposing the whole DbContext
    IQueryable<TEntity> Query();

    // Read-only queries: skips change-tracking overhead (no entity updates)
    IQueryable<TEntity> QueryNoTracking();

    // Tracked queries: use when you load an entity to modify/delete it
    IQueryable<TEntity> QueryTracked();

    Task<TEntity?> GetByIdAsync(object id);
    Task<List<TEntity>> ListAsync();
    void Add(TEntity entity);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}