namespace Simando.Domain.Nol;

public sealed class NolEvaluationScenario
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolEvaluationNolRequestId { get; set; }

    public required string Label { get; set; }
    public decimal? IrrPct { get; set; }
    public decimal? Npv { get; set; }
    public decimal? PaybackYears { get; set; }
    public string? HasilAnalisis { get; set; }
}

public sealed class EvaluationResume
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid NolEvaluationNolRequestId { get; set; }
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid GeneratedByUserId { get; set; }
    public Guid AttachmentId { get; set; }
}

// Stage 7 (NOL Evaluation) header — 1:1 extension of NolRequest, NolRequestId is the primary key.
// docs/design/data-model.md#nol_evaluation--stage-7
public sealed class NolEvaluation
{
    public required Guid NolRequestId { get; init; }

    public FeedStatus FeedStatus { get; set; } = FeedStatus.Belum;
    public DateOnly? FeedCompletedAt { get; set; }

    public decimal? CapexFinal { get; set; }

    public decimal? PipaIndukPanjangM { get; set; }
    public decimal? PipaIndukDiameter { get; set; }
    public DiameterUnit? PipaIndukDiameterUnit { get; set; }

    public decimal? PipaServicePanjangM { get; set; }
    public decimal? PipaServiceDiameter { get; set; }
    public DiameterUnit? PipaServiceDiameterUnit { get; set; }

    public string? SpesifikasiMrs { get; set; }
    public string? GSize { get; set; }
    public decimal? Tekanan { get; set; }
    public decimal? MaksFlowrate { get; set; }

    public decimal? MaksKapasitasMeterM3Jam { get; set; }
    public short? DurasiPelaksanaanBulan { get; set; }

    public StatusRkap? StatusRkap { get; set; }
    public SkemaPembayaran? SkemaPembayaran { get; set; }

    public string? JaminanStatus { get; set; }
    public string? JaminanJenis { get; set; }
    public string? JaminanMasaBerlaku { get; set; }
    public string? JaminanPenerbit { get; set; }

    public decimal? KetersediaanPasokanBbtud { get; set; }

    public string? AnalisisKomersial { get; set; }
    public string? AnalisisKompetitor { get; set; }
    public string? Kesimpulan { get; set; }
    public decimal? RadiusKompetitorKm { get; set; }

    public Guid? EvaluatedBy { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }

    public List<NolEvaluationScenario> Scenarios { get; set; } = [];
}
