namespace Simando.Domain.Nol;

public sealed class NolIssuanceApprovedTerm
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolIssuanceNolRequestId { get; set; }

    public DateOnly PeriodeMulai { get; set; }
    public DateOnly PeriodeSelesai { get; set; }
    public decimal RataRata { get; set; }
    public decimal KontrakMinimum { get; set; }
    public decimal KontrakMaksimum { get; set; }
    public short SortOrder { get; set; }
}

// Stage 8 (NOL Issuance) header — 1:1 extension of NolRequest, NolRequestId is primary key.
// docs/design/data-model.md#nol_issuance--stage-8
public sealed class NolIssuance
{
    public required Guid NolRequestId { get; init; }

    public NolOutcome Outcome { get; set; } = NolOutcome.Nol;
    public string? NomorNotaDinas { get; set; }

    public List<string> KontrakBersyarat { get; set; } = [];

    public DateOnly? BerlakuSejak { get; set; }
    public DateOnly? BerlakuSampai { get; set; }

    public Guid? SignedByUserId { get; set; }
    public DateTimeOffset? SignedAt { get; set; }

    public Guid? DocumentId { get; set; }

    public List<NolIssuanceApprovedTerm> ApprovedTerms { get; set; } = [];
}
