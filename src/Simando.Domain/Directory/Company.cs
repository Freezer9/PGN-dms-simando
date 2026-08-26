using NetTopologySuite.Geometries;
using Simando.Domain.Workflow;

namespace Simando.Domain.Directory;

// The stage-1 spine every other stage's data hangs off.
// docs/design/data-model.md#company.
public sealed class Company
{
    public required Guid Id { get; init; }

    // From the global company_nomor_seq sequence — the real identity.
    // Nomor is the rendered "{seq}-{bps_prov}-{bps_kab}" string, re-rendered
    // while Draft and frozen once the record leaves Draft (it appears on
    // signed documents). docs/domain/03-directory-plotting.md#the-nomor-format.
    public int NomorSeq { get; init; }
    public required string Nomor { get; set; }

    public required string NamaPerusahaan { get; set; }
    public string? Website { get; set; }

    public required Guid VillageId { get; set; }
    public required string Alamat { get; set; }

    // geography(Point,4326) — manual pin-drop. Not two decimal columns: the
    // map does radius and bbox queries.
    public Point? Location { get; set; }

    public required Guid IndustryTypeId { get; set; }
    public string? Npwp { get; set; }
    public string? Email { get; set; }
    public string? KodePos { get; set; }
    public string? Telp { get; set; }
    public string? Fax { get; set; }

    public required Guid AreaId { get; set; }
    public required byte CurrentStage { get; set; }
    public required RecordStatus Status { get; set; }

    public required Guid CreatedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
