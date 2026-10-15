using System.ComponentModel.DataAnnotations;

namespace Eventix.Application.DTOs.Categories;

/// <summary>Input for creating or updating a category.</summary>
public class CategoryRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
