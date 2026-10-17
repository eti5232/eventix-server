using Eventix.Application.Common;
using Eventix.Application.DTOs.Equipment;
using Eventix.Application.Interfaces;
using Eventix.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventix.API.Controllers;

/// <summary>The equipment catalog. The controller only handles HTTP; the logic lives in <see cref="IEquipmentService"/>.</summary>
[ApiController]
[Route("api/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentService _equipment;

    public EquipmentController(IEquipmentService equipment)
    {
        _equipment = equipment;
    }

    /// <summary>Lists active equipment with filtering, sorting and paging.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EquipmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged([FromQuery] EquipmentQuery query, CancellationToken cancellationToken) =>
        Ok(await _equipment.GetPagedAsync(query, cancellationToken));

    /// <summary>Gets one equipment item.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken) =>
        Ok(await _equipment.GetByIdAsync(id, cancellationToken));

    /// <summary>Creates an equipment item.</summary>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EquipmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] EquipmentRequestDto request, CancellationToken cancellationToken)
    {
        var created = await _equipment.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an equipment item.</summary>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(EquipmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] int id, [FromBody] EquipmentRequestDto request, CancellationToken cancellationToken) =>
        Ok(await _equipment.UpdateAsync(id, request, cancellationToken));

    /// <summary>Soft-deletes an equipment item (it is hidden from the catalog but kept in the database).</summary>
    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await _equipment.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
