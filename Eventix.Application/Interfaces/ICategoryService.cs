using Eventix.Application.DTOs.Categories;

namespace Eventix.Application.Interfaces;

/// <summary>Business logic for categories.</summary>
public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The category does not exist.</exception>
    Task<CategoryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.ConflictException">A category with this name already exists.</exception>
    Task<CategoryResponseDto> CreateAsync(CategoryRequestDto request, CancellationToken cancellationToken);

    /// <exception cref="Exceptions.NotFoundException">The category does not exist.</exception>
    /// <exception cref="Exceptions.ConflictException">Another category already has this name.</exception>
    Task<CategoryResponseDto> UpdateAsync(int id, CategoryRequestDto request, CancellationToken cancellationToken);
}
