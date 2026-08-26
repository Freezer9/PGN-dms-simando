using Microsoft.EntityFrameworkCore;
using Simando.Domain.MasterData;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.MasterData;

// Go-live checklist items #4-#8, docs/domain/master-data.md #13 -- the
// small, literal lookup lists whose source is this document (or ISO
// 3166-1) rather than an external dataset. Geography (#2) is big enough to
// need its own seeder/data files -- see GeographySeeder. Each table is
// gated independently (AnyAsync) so a partial prior run or a fresh table
// added later can still be seeded without re-inserting existing rows.
public sealed class MasterDataSeeder(SimandoDbContext db)
{
    // docs/domain/master-data.md #7 "Satuan" -- the 17 distinct units behind
    // all eight <MeasureInput Set="..."> dropdowns, deduplicated (e.g. TR
    // backs both Capacity and Cooling as the same physical unit).
    private static readonly (string Code, string Name, UnitDimension Dimension)[] Units =
    [
        ("MW", "MW", UnitDimension.Power),
        ("Ton/Jam", "Ton/Jam", UnitDimension.Flow),
        ("Kkal", "Kkal", UnitDimension.Energy),
        ("TR", "TR", UnitDimension.Power),
        ("PK", "PK", UnitDimension.Power),
        ("Kw", "Kw", UnitDimension.Power),
        ("Kwh", "Kwh", UnitDimension.Energy),
        ("Ton", "Ton", UnitDimension.Mass),
        ("Liter", "Liter", UnitDimension.Volume),
        ("%", "%", UnitDimension.Ratio),
        ("KL", "KL", UnitDimension.Volume),
        ("m2", "m²", UnitDimension.Area),
        ("Inch", "Inch", UnitDimension.Length),
        ("mm", "mm", UnitDimension.Length),
        ("m3", "m³", UnitDimension.Volume),
        ("MMBtu", "MMBtu", UnitDimension.Energy),
        ("barg", "barg", UnitDimension.Pressure),
    ];

    // Which units populate which dropdown, and in what order -- the doc's
    // §7 table, one row per set.
    private static readonly (UnitSet Set, string[] Codes)[] UnitSetMembers =
    [
        (UnitSet.Capacity, ["MW", "Ton/Jam", "Kkal", "TR"]),
        (UnitSet.Cooling, ["TR", "PK", "Kw"]),
        (UnitSet.EnergyUsage, ["Kwh", "Ton", "Kkal", "TR"]),
        (UnitSet.FuelConsumption, ["Ton", "Liter", "Kwh"]),
        (UnitSet.RawMaterial, ["%", "Ton", "KL", "m2"]),
        (UnitSet.Diameter, ["Inch", "mm"]),
        (UnitSet.GasVolume, ["m3", "MMBtu"]),
        (UnitSet.Pressure, ["barg"]),
    ];

    // docs/domain/master-data.md #7 "Jenis Bahan Bakar" -- union of the KK0
    // and Lampiran 10 lists, including 'Lainnya'.
    private static readonly string[] FuelTypes =
    [
        "LPG", "CNG", "HSD (Solar)", "MFO", "Minyak Berat (FO)", "IDO", "MDF",
        "Minyak Tanah", "Batubara", "Cangkang", "Kayu", "Listrik", "Lainnya",
    ];

    // docs/domain/master-data.md #5 "Jenis Industri" -- curated 20-category
    // list with example products.
    private static readonly (string Name, string ContohProduk)[] IndustryTypes =
    [
        ("Makanan Minuman", "Wafer, Biskuit"),
        ("Logam Dasar Non Baja", "Profil Aluminium"),
        ("Logam Dasar Baja", "Besi Beton"),
        ("Fabrikasi Logam Non Baja", "Furniture"),
        ("Fabrikasi Logam Baja", "Otomotif, Heat Exchanger"),
        ("Bahan Tekstil", "Kain Sarung, Kain Pakaian"),
        ("Kertas", "Karton, Packaging"),
        ("Kaca", "Kaca Otomotif, Kaca Lembaran, Gelas"),
        ("Ceramic", "Keramik Lantai, Keramik Sanitary, Genteng"),
        ("CNG/LNG", "CNG, LNG"),
        ("Kimia", "Stereofoam, Sabun"),
        ("Smelter", "Smelter Emas, Smelter Aluminium"),
        ("Rubber", "Ban, Balon"),
        ("Plastic", "Alat Rumah Tangga, Packaging"),
        ("Laundry", "Jasa Laundry"),
        ("Tobacco", "Rokok"),
        ("Wood", "Furniture, Floring"),
        ("Farmasi", "Obat, Infus"),
        ("Gas Industri", "H2, O2, CO2"),
        ("Horeka", "Hotel, Resto, Kafe"),
    ];

    // docs/domain/master-data.md #6 "Segmen (Sub-Produk)" -- fixed tier
    // order, worst to best.
    private static readonly string[] Segments = ["Bronze 1", "Bronze 2", "Bronze 3", "Silver", "Gold", "Platinum"];

    public async Task<MasterDataSeedResult> SeedAsync(CancellationToken ct = default)
    {
        var units = await SeedUnitsAsync(ct);
        var fuelTypes = await SeedSimpleAsync(db.FuelTypes, FuelTypes, name => new FuelType { Id = Guid.NewGuid(), Name = name }, ct);
        var industryTypes = await SeedIndustryTypesAsync(ct);
        var countries = await SeedSimpleAsync(
            db.Countries, CountrySeedData.Countries,
            c => new Country { Id = Guid.NewGuid(), IsoCode = c.IsoCode, Name = c.Name }, ct);
        var segments = await SeedSegmentsAsync(ct);

        return new MasterDataSeedResult(units, fuelTypes, industryTypes, countries, segments);
    }

    private async Task<int> SeedUnitsAsync(CancellationToken ct)
    {
        if (await db.UnitsOfMeasure.AnyAsync(ct))
        {
            return 0;
        }

        var unitIds = new Dictionary<string, Guid>();
        foreach (var (code, name, dimension) in Units)
        {
            var id = Guid.NewGuid();
            unitIds[code] = id;
            db.UnitsOfMeasure.Add(new UnitOfMeasure { Id = id, Code = code, Name = name, Dimension = dimension });
        }

        foreach (var (set, codes) in UnitSetMembers)
        {
            for (var i = 0; i < codes.Length; i++)
            {
                db.UnitSetMembers.Add(new UnitSetMember { Id = Guid.NewGuid(), SetCode = set, UnitId = unitIds[codes[i]], SortOrder = i + 1 });
            }
        }

        await db.SaveChangesAsync(ct);
        return Units.Length;
    }

    private async Task<int> SeedIndustryTypesAsync(CancellationToken ct)
    {
        if (await db.IndustryTypes.AnyAsync(ct))
        {
            return 0;
        }

        db.IndustryTypes.AddRange(IndustryTypes.Select(i => new IndustryType { Id = Guid.NewGuid(), Name = i.Name, ContohProduk = i.ContohProduk }));
        await db.SaveChangesAsync(ct);
        return IndustryTypes.Length;
    }

    private async Task<int> SeedSegmentsAsync(CancellationToken ct)
    {
        if (await db.Segments.AnyAsync(ct))
        {
            return 0;
        }

        db.Segments.AddRange(Segments.Select((name, i) => new Segment { Id = Guid.NewGuid(), Name = name, SortOrder = i + 1 }));
        await db.SaveChangesAsync(ct);
        return Segments.Length;
    }

    private async Task<int> SeedSimpleAsync<TSource, TEntity>(
        DbSet<TEntity> set, TSource[] source, Func<TSource, TEntity> map, CancellationToken ct)
        where TEntity : class
    {
        if (await set.AnyAsync(ct))
        {
            return 0;
        }

        set.AddRange(source.Select(map));
        await db.SaveChangesAsync(ct);
        return source.Length;
    }
}

public sealed record MasterDataSeedResult(int Units, int FuelTypes, int IndustryTypes, int Countries, int Segments);
