namespace Eventix.Application.DTOs.Categories;

/// <summary>A category as returned by the API.</summary>
public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
