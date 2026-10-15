namespace Eventix.Domain.Common;

/// <summary>
/// An entity that carries an optimistic-concurrency token.
/// The infrastructure replaces <see cref="Version"/> on every update, so a stale write matches no row.
/// </summary>
public interface IVersioned
{
    Guid Version { get; set; }
}
