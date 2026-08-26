using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Segmen / Sub-Produk — Bronze 1-3, Silver, Gold, Platinum.
// docs/domain/master-data.md §6. Not priced here: harga lives on
// a1_registration/nol_request directly, typed per record.
public sealed class Segment : AuditableEntity
{
    public required string Name { get; set; }
    public required int SortOrder { get; set; }
}
