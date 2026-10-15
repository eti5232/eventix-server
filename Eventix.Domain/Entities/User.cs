using Eventix.Domain.Enums;

namespace Eventix.Domain.Entities;

/// <summary>A registered user: either a producer or the warehouse admin.</summary>
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    /// <summary>Unique, always stored in lowercase.</summary>
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
