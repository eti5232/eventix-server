using System.ComponentModel.DataAnnotations;
using Eventix.Application.Common;
using Eventix.Domain.Enums;

namespace Eventix.Application.DTOs.Reservations;

/// <summary>Filter and paging parameters of the reservations list, bound from the query string.</summary>
public class ReservationQuery
{
    /// <summary>Only reservations in this status (optional).</summary>
    public ReservationStatus? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, Paging.MaxPageSize)]
    public int PageSize { get; set; } = Paging.DefaultPageSize;
}
