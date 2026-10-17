using Eventix.Domain.Entities;

namespace Eventix.Application.Interfaces;

/// <summary>Data access for the daily stock rows, the resource that users compete on. Saving is done through <see cref="IUnitOfWork"/>.</summary>
public interface IDailyStockRepository
{
    /// <summary>The existing, tracked stock rows of these equipment items in the range (inclusive). Days without a row are not returned.</summary>
    Task<IReadOnlyList<DailyStock>> GetForRangeAsync(
        IReadOnlyCollection<int> equipmentIds, DateOnly start, DateOnly end, CancellationToken cancellationToken);

    /// <summary>
    /// A tracked stock row for every equipment item and every day of the range (inclusive): the existing ones, plus new
    /// rows (reserved 0, added to the context) for the days that have none yet. Uses one query, not one per day.
    /// </summary>
    Task<IReadOnlyList<DailyStock>> GetOrCreateForRangeAsync(
        IReadOnlyCollection<int> equipmentIds, DateOnly start, DateOnly end, CancellationToken cancellationToken);

    /// <summary>The highest reserved quantity of one equipment item on any day of the range (inclusive), read-only. 0 if there are no rows.</summary>
    Task<int> GetMaxReservedQuantityAsync(int equipmentId, DateOnly start, DateOnly end, CancellationToken cancellationToken);

    /// <summary>The highest reserved quantity of one equipment item on any day from <paramref name="fromDate"/> onward, read-only. 0 if there are no rows.</summary>
    Task<int> GetMaxReservedQuantityFromAsync(int equipmentId, DateOnly fromDate, CancellationToken cancellationToken);
}
