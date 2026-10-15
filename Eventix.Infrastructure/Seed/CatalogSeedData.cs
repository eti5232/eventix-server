using Eventix.Domain.Entities;

namespace Eventix.Infrastructure.Seed;

/// <summary>
/// Fixed catalog data applied through <c>HasData</c>, so the system is usable right after the first migration.
/// Items with quantity 2 make it easy to demonstrate a booking conflict.
/// </summary>
internal static class CatalogSeedData
{
    private const int Speakers = 1, Microphones = 2, Lighting = 3, Projectors = 4, Stages = 5;

    public static readonly Category[] Categories =
    {
        new() { Id = Speakers, Name = "Speakers", Description = "PA systems, subwoofers and stage monitors" },
        new() { Id = Microphones, Name = "Microphones", Description = "Wireless, lavalier and conference microphones" },
        new() { Id = Lighting, Name = "Lighting", Description = "LED, moving head and follow spot lights" },
        new() { Id = Projectors, Name = "Projectors", Description = "Projectors and projection screens" },
        new() { Id = Stages, Name = "Stages", Description = "Stage platforms, trusses and risers" }
    };

    public static readonly Equipment[] Equipment =
    {
        Item(1, Speakers, "PA Speaker 1000W", 85m, 10),
        Item(2, Speakers, "Subwoofer 18 inch", 120m, 6),
        Item(3, Speakers, "Stage Monitor Speaker", 45m, 8),
        Item(4, Microphones, "Wireless Handheld Microphone", 40m, 12),
        Item(5, Microphones, "Lavalier Microphone", 35m, 8),
        Item(6, Microphones, "Conference Gooseneck Microphone", 30m, 2),
        Item(7, Lighting, "LED Par Light", 25m, 24),
        Item(8, Lighting, "Moving Head Light", 90m, 8),
        Item(9, Lighting, "Follow Spot", 150m, 2),
        Item(10, Projectors, "Full HD Projector 5000lm", 200m, 4),
        Item(11, Projectors, "4K Laser Projector", 450m, 2),
        Item(12, Projectors, "Projection Screen 3m", 70m, 5),
        Item(13, Stages, "Stage Platform 2x1m", 60m, 20),
        Item(14, Stages, "Truss Section 3m", 55m, 12),
        Item(15, Stages, "Drum Riser", 80m, 2)
    };

    private static Equipment Item(int id, int categoryId, string name, decimal dailyPrice, int totalQuantity) => new()
    {
        Id = id,
        CategoryId = categoryId,
        Name = name,
        DailyPrice = dailyPrice,
        TotalQuantity = totalQuantity,
        IsActive = true
    };
}
