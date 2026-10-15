using Eventix.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eventix.Infrastructure.Configurations;

internal class DailyStockConfiguration : IEntityTypeConfiguration<DailyStock>
{
    /// <summary>Name of the unique index; the UnitOfWork recognises a duplicate-row race by it.</summary>
    public const string UniqueIndexName = "IX_DailyStocks_EquipmentId_Date";

    public void Configure(EntityTypeBuilder<DailyStock> builder)
    {
        builder.HasKey(s => s.Id);

        // The optimistic-concurrency token: EF adds the old Version to the UPDATE ... WHERE clause.
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.HasIndex(s => new { s.EquipmentId, s.Date })
            .IsUnique()
            .HasDatabaseName(UniqueIndexName);

        builder.HasOne(s => s.Equipment)
            .WithMany(e => e.DailyStocks)
            .HasForeignKey(s => s.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
