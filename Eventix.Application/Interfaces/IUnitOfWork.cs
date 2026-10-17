namespace Eventix.Application.Interfaces;

/// <summary>Commits all changes made through the repositories as one unit.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs the action inside one database transaction: it is committed when the action completes and rolled back if it throws.
    /// Row locks taken inside the action (see <see cref="IEquipmentRepository.LockForShareAsync"/>) are held until the transaction ends.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken);

    /// <inheritdoc cref="ExecuteInTransactionAsync{T}"/>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken);
}
