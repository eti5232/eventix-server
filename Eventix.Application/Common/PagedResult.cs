namespace Eventix.Application.Common;

/// <summary>One page of a list, together with the total number of items across all pages.</summary>
/// <param name="Items">The items of the requested page.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Maximum number of items per page.</param>
/// <param name="TotalCount">Number of items matching the query, ignoring paging.</param>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
