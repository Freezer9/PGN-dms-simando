using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Bahan Baku (Negara Asal) / Orientasi Pasar (Negara Tujuan) — seeded from
// ISO 3166-1 with Indonesian names, docs/domain/master-data.md §4 "Negara".
public sealed class Country : AuditableEntity
{
    public required string IsoCode { get; set; }
    public required string Name { get; set; }
}
