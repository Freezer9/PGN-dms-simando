using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Simando.Domain.Geography;
using Simando.Infrastructure.Persistence;

namespace Simando.Infrastructure.Geography;

// Go-live checklist item #2, docs/domain/master-data.md #4 "Administrative
// geography" -- Province/Regency/District/Village, seeded once from bundled
// Kemendagri-code data (github.com/cahyadsn/wilayah's db/wilayah.sql, see
// Persistence/SeedData/Geography/README.md) rather than typed by hand.
// "No admin UI for this table" is a deliberate call in that doc, so this
// import is the only way the ~91,600 rows get in.
//
// Each source file is "{code}|{name}" per line, one row per level, where
// `code` is wilayah.id's dotted Kemendagri code (e.g. "35.78.13.1001") --
// the parent's code is always the same string with its last "." segment
// removed, so the hierarchy is walked from the codes themselves without a
// separate parent column. Files are embedded resources so this works from
// any working directory / published image, and are streamed line-by-line
// (never loaded whole) since villages.csv alone is ~84,000 rows.
public sealed class GeographySeeder(SimandoDbContext db)
{
    public async Task<GeographySeedResult> SeedAsync(CancellationToken ct = default)
    {
        if (await db.Provinces.AnyAsync(ct))
        {
            return new GeographySeedResult(AlreadySeeded: true, 0, 0, 0, 0);
        }

        var provinceIds = new Dictionary<string, Guid>();
        var provinces = new List<Province>();
        foreach (var (code, name) in ReadRows("provinces.csv"))
        {
            var id = Guid.NewGuid();
            provinceIds[code] = id;
            provinces.Add(new Province { Id = id, BpsCode = code, Name = name });
        }

        db.Provinces.AddRange(provinces);
        await db.SaveChangesAsync(ct);

        var regencyIds = new Dictionary<string, Guid>();
        var regencyCount = 0;
        await foreach (var batch in BatchAsync(ReadRows("regencies.csv"), 2000))
        {
            var entities = new List<Regency>(batch.Count);
            foreach (var (code, rawName) in batch)
            {
                var (type, name) = SplitRegencyPrefix(rawName);
                var id = Guid.NewGuid();
                regencyIds[code] = id;
                entities.Add(new Regency { Id = id, ProvinceId = provinceIds[ParentCode(code)], BpsCode = LocalSegment(code), Type = type, Name = name });
            }

            db.Regencies.AddRange(entities);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
            regencyCount += entities.Count;
        }

        var districtIds = new Dictionary<string, Guid>();
        var districtCount = 0;
        await foreach (var batch in BatchAsync(ReadRows("districts.csv"), 2000))
        {
            var entities = new List<District>(batch.Count);
            foreach (var (code, name) in batch)
            {
                var id = Guid.NewGuid();
                districtIds[code] = id;
                entities.Add(new District { Id = id, RegencyId = regencyIds[ParentCode(code)], BpsCode = LocalSegment(code), Name = name });
            }

            db.Districts.AddRange(entities);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
            districtCount += entities.Count;
        }

        var villageCount = 0;
        await foreach (var batch in BatchAsync(ReadRows("villages.csv"), 5000))
        {
            var entities = new List<Village>(batch.Count);
            foreach (var (code, name) in batch)
            {
                var local = LocalSegment(code);
                // Kemendagri convention: the village code's local segment
                // starts '1' for Kelurahan, '2' for Desa -- independent of
                // whether the parent is a Kota or Kabupaten (e.g. Kuta in
                // Kabupaten Badung is Kelurahan-coded). A '3' prefix shows
                // up for 14 Papua villages ("Desa Adat" / customary desa) --
                // VillageType has no third case for it, so it falls into
                // Desa, the closer match.
                var type = local.StartsWith('1') ? VillageType.Kelurahan : VillageType.Desa;
                entities.Add(new Village { Id = Guid.NewGuid(), DistrictId = districtIds[ParentCode(code)], BpsCode = local, Type = type, Name = name });
            }

            db.Villages.AddRange(entities);
            await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
            villageCount += entities.Count;
        }

        return new GeographySeedResult(AlreadySeeded: false, provinces.Count, regencyCount, districtCount, villageCount);
    }

    private static string ParentCode(string code) => code[..code.LastIndexOf('.')];

    private static string LocalSegment(string code) => code[(code.LastIndexOf('.') + 1)..];

    // wilayah.id names the row "Kota Surabaya" / "Kabupaten Sidoarjo" --
    // Type carries that distinction, so Name is stored without the prefix
    // (docs/domain/master-data.md #4: "'Surabaya', without the prefix").
    private static (RegencyType Type, string Name) SplitRegencyPrefix(string rawName)
    {
        if (rawName.StartsWith("Kota ", StringComparison.Ordinal))
        {
            return (RegencyType.Kota, rawName["Kota ".Length..]);
        }

        if (rawName.StartsWith("Kabupaten ", StringComparison.Ordinal))
        {
            return (RegencyType.Kabupaten, rawName["Kabupaten ".Length..]);
        }

        return (RegencyType.Kabupaten, rawName);
    }

    private static IEnumerable<(string Code, string Name)> ReadRows(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Simando.Infrastructure.Persistence.SeedData.Geography.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded seed resource not found: {resourceName}");
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length == 0)
            {
                continue;
            }

            var separator = line.IndexOf('|');
            yield return (line[..separator], line[(separator + 1)..]);
        }
    }

    private static async IAsyncEnumerable<List<(string Code, string Name)>> BatchAsync(IEnumerable<(string Code, string Name)> source, int size)
    {
        var batch = new List<(string Code, string Name)>(size);
        foreach (var row in source)
        {
            batch.Add(row);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<(string Code, string Name)>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }

        await Task.CompletedTask;
    }
}

public sealed record GeographySeedResult(bool AlreadySeeded, int Provinces, int Regencies, int Districts, int Villages);
