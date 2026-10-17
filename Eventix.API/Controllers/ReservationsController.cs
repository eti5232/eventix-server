using Eventix.Application.Common;
using Eventix.Application.DTOs.Reservations;
using Eventix.Application.Interfaces;
using Eventix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventix.API.Controllers;

/// <summary>Equipment reservations. The controller only handles HTTP; the logic lives in <see cref="IReservationService"/>.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;

    public ReservationsController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    /// <summary>Reserves equipment for a date range (producers only). All items are reserved, or none.</summary>
    [Authorize(Roles = nameof(UserRole.Producer))]
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _reservations.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Changes the status: Cancelled (owner or admin, before the start; gives the stock back) or Returned (admin only, once started;
    /// an early return frees the stock from the following day).
    /// </summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] int id, [FromBody] UpdateReservationStatusRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _reservations.ChangeStatusAsync(id, request.Status, cancellationToken));

    /// <summary>Lists reservations, newest first. A producer sees only their own; an admin sees all.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaged([FromQuery] ReservationQuery query, CancellationToken cancellationToken) =>
        Ok(await _reservations.GetPagedAsync(query, cancellationToken));

    /// <summary>Gets one reservation (its owner or an admin).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _reservations.GetByIdAsync(id, cancellationToken));
}
