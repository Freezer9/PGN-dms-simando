namespace Simando.Domain.Directory;

// Stage 2, a 1:1 extension of Company — CompanyId is the primary key, no
// surrogate id (the doc's column list has no id column for this table).
// docs/design/data-model.md#plotting--stage-2.
public sealed class Plotting
{
    public required Guid CompanyId { get; init; }
    public required Guid SalesUserId { get; set; }
    public required PosisiPelanggan PosisiPelanggan { get; set; }
    public required Kawasan Kawasan { get; set; }
}
