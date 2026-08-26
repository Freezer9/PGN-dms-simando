namespace Simando.Domain.Directory;

// Stage 3, repeating — minimum 1 per Company. Social handles are
// deliberate: sales use them to research decision-makers.
// docs/design/data-model.md#company_contact--stage-3-repeating.
public sealed class CompanyContact
{
    public required Guid Id { get; init; }
    public required Guid CompanyId { get; init; }

    public required string Nama { get; set; }
    public required string Jabatan { get; set; }

    public string? Email { get; set; }
    public string? NoHp { get; set; }
    public string? LinkedIn { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }

    public required bool IsPrimary { get; set; }
    public required short SortOrder { get; set; }
}
