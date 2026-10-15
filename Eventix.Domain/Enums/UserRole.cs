namespace Eventix.Domain.Enums;

/// <summary>Authorization role of a user.</summary>
public enum UserRole
{
    /// <summary>Event producer who rents equipment.</summary>
    Producer,

    /// <summary>Warehouse manager who maintains the catalog.</summary>
    Admin
}
