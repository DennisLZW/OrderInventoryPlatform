namespace OrderInventoryPlatform.Application.Interfaces;

/// <summary>
/// Unit of work for committing a single transaction across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the delegate inside one database transaction. Use for operations that must call
    /// <see cref="SaveChangesAsync"/> exactly once and commit atomically (e.g. place order).
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<Task> action,
        CancellationToken cancellationToken = default);
}
