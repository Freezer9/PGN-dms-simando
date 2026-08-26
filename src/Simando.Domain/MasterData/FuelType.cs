using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Jenis Bahan Bakar — union of the KK0 and Lampiran 10 lists, including
// 'Lainnya'. docs/domain/master-data.md §7. Stays master data (not a fixed
// enum) because the two source lists already disagreed once.
public sealed class FuelType : AuditableEntity
{
    public required string Name { get; set; }
}
