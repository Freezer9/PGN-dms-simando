using Simando.Domain.Common;

namespace Simando.Domain.MasterData;

// G-Size gas meter catalogue. docs/domain/master-data.md §8. Selecting a
// G-Size populates max flowrate rather than leaving it free-typed.
public sealed class MeterSize : AuditableEntity
{
    public required string GSize { get; set; }
    public required decimal NominalFlow { get; set; }
    public required decimal MaxFlow { get; set; }
    public required decimal PressureRating { get; set; }
}
