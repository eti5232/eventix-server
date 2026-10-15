namespace Eventix.Domain.Entities;

/// <summary>A group of equipment, such as speakers or lighting.</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();
}
