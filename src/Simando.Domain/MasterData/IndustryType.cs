using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Jenis Industri — curated 20-category list (or BPS/KBLI if PGN supplies
// it), extensible master table. docs/domain/master-data.md §5. KBLI code
// lives per-Company, not here.
public sealed class IndustryType : AuditableEntity
{
    public required string Name { get; set; }
    public string? ContohProduk { get; set; }
}
