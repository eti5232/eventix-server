using AutoMapper;
using Eventix.Application.Common;
using Eventix.Application.DTOs.Equipment;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;

namespace Eventix.Application.Services;

/// <summary>Business logic for the equipment catalog.</summary>
public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository _equipment;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EquipmentService(
        IEquipmentRepository equipment,
        ICategoryRepository categories,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _equipment = equipment;
        _categories = categories;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<EquipmentResponseDto>> GetPagedAsync(
        EquipmentQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _equipment.GetPagedAsync(query, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<EquipmentResponseDto>>(items);
        return new PagedResult<EquipmentResponseDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<EquipmentResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var equipment = await _equipment.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Equipment", id);
        return _mapper.Map<EquipmentResponseDto>(equipment);
    }

    public async Task<EquipmentResponseDto> CreateAsync(EquipmentRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var equipment = _mapper.Map<Equipment>(request);
        await _equipment.AddAsync(equipment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(equipment.Id, cancellationToken);
    }

    public async Task<EquipmentResponseDto> UpdateAsync(int id, EquipmentRequestDto request, CancellationToken cancellationToken)
    {
        var equipment = await _equipment.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Equipment", id);
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        _mapper.Map(request, equipment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Re-read so the response always shows the saved state, including the (possibly new) category name.
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var equipment = await _equipment.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Equipment", id);

        // Soft delete. Stage 6 adds the rule that blocks removal while future reservations exist.
        equipment.IsActive = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        if (!await _categories.ExistsAsync(categoryId, cancellationToken))
            throw new NotFoundException("Category", categoryId);
    }
}
