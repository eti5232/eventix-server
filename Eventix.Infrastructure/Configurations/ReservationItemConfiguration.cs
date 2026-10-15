using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.Infrastructure.Configurations;

/// <summary>Configures the join entity that makes Reservation and Equipment a many-to-many relation.</summary>
internal class ReservationItemConfiguration : IEntityTypeConfiguration<ReservationItem>
{
    public void Configure(EntityTypeBuilder<ReservationItem> builder)
    {
        builder.HasKey(i => new { i.ReservationId, i.EquipmentId });
        builder.Property(i => i.UnitPrice).HasPrecision(10, 2);

        builder.HasOne(i => i.Reservation)
            .WithMany(r => r.Items)
            .HasForeignKey(i => i.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Equipment)
            .WithMany(e => e.ReservationItems)
            .HasForeignKey(i => i.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
