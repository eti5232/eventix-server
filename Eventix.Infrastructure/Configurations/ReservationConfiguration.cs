using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.Infrastructure.Configurations;

internal class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.EventName).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.TotalPrice).HasPrecision(12, 2);

        // Replaced on every update (by AppDbContext), so a stale status change matches no row.
        builder.Property(r => r.Version).IsConcurrencyToken();

        // Used by the "does this equipment have reservations that have not ended yet?" check.
        builder.HasIndex(r => new { r.Status, r.EndDate });

        // Safety net in the database itself, in case of a bug in the code.
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Reservations_DateRange", "\"EndDate\" >= \"StartDate\"");

            // Equipment cannot be returned before the reservation started.
            table.HasCheckConstraint("CK_Reservations_ReturnedOn_NotBeforeStart", "\"ReturnedOn\" IS NULL OR \"ReturnedOn\" >= \"StartDate\"");
        });
    }
}
