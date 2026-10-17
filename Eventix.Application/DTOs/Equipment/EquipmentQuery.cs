using System.ComponentModel.DataAnnotations;
using Eventix.Application.Common;

namespace Eventix.Application.DTOs.Equipment;

/// <summary>Filter, sort and paging parameters of the equipment list, bound from the query string.</summary>
public class EquipmentQuery
{
    public int? CategoryId { get; set; }

    /// <summary>Case-insensitive text matched against the name and the description.</summary>
    public string? Search { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    public EquipmentSortBy SortBy { get; set; } = EquipmentSortBy.Name;
    public SortDirection SortDir { get; set; } = SortDirection.Asc;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, Paging.MaxPageSize)]
    public int PageSize { get; set; } = Paging.DefaultPageSize;
}
