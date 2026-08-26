using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// Spesifikasi MRS catalogue — stops the same station being written five
// ways across five records. docs/domain/master-data.md §8.
public sealed class MrsSpec : AuditableEntity
{
    public required string Name { get; set; }
}
