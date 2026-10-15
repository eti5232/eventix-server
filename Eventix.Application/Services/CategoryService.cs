using AutoMapper;
using Eventix.Application.DTOs.Categories;
using Eventix.Application.Exceptions;
using Eventix.Application.Interfaces;
using Eventix.Domain.Entities;

namespace Eventix.Application.Services;

/// <summary>Business logic for categories.</summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categories, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _categories = categories;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _categories.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
    }

    public async Task<CategoryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category", id);
        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto request, CancellationToken cancellationToken)
    {
        await EnsureNameIsFreeAsync(request.Name, excludeId: null, cancellationToken);

        var category = _mapper.Map<Category>(request);
        await _categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponseDto>(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(int id, CategoryRequestDto request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category", id);
        await EnsureNameIsFreeAsync(request.Name, excludeId: id, cancellationToken);

        _mapper.Map(request, category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponseDto>(category);
    }

    private async Task EnsureNameIsFreeAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        if (await _categories.NameExistsAsync(name, excludeId, cancellationToken))
            throw new ConflictException($"A category named '{name.Trim()}' already exists.");
    }
}
