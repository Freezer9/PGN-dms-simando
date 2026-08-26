namespace Simando.Domain.Survey;

// Stage 4 (KK0) header — 1:1 extension of Company, CompanyId is the primary
// key, no surrogate id (same shape as Directory.Plotting).
// docs/design/data-model.md#survey--stage-4-kk0-header.
//
// beban_puncak (peak-load windows) is deliberately absent — client-sheet-only
// field, tracked as its own lower-priority backlog card
// (.devtool/features/web-beban-puncak-peak-load-capture-2026-08-07.md).
public sealed class Survey
{
    public required Guid CompanyId { get; init; }

    public DateOnly? TanggalSurvey { get; set; }
    public Guid? SurveyorUserId { get; set; }

    public int? JumlahKaryawan { get; set; }
    public short? JumlahShift { get; set; }
    public decimal? JamKerjaPerHari { get; set; }
    public short? HariPerMinggu { get; set; }

    public TimeOnly? BebanPuncak1Mulai { get; set; }
    public TimeOnly? BebanPuncak1Selesai { get; set; }
    public TimeOnly? BebanPuncak2Mulai { get; set; }
    public TimeOnly? BebanPuncak2Selesai { get; set; }

    public KebutuhanEnergiJenis? KebutuhanEnergi { get; set; }
    public string? KebutuhanEnergiLainnya { get; set; }
    public decimal? KapasitasNilai { get; set; }
    public Guid? KapasitasUnitId { get; set; }
    public decimal? PemakaianNilai { get; set; }
    public Guid? PemakaianUnitId { get; set; }

    // Only field marked required (✱) in this section of the source, but
    // not NOT-NULL: the form is autosave/non-wizard, so a desk transcription
    // session can be interrupted before this section is reached. "Required"
    // is submit-time validation, not a DB constraint.
    public decimal? PipaTerdekatJarakM { get; set; }
    public decimal? PipaTerdekatDiameter { get; set; }
    public decimal? PipaTerdekatTekanan { get; set; }

    public BahanBakarEksisting? BahanBakarEksisting { get; set; }
    public string? NamaPemasok { get; set; }
    public decimal? KapasitasListrikKw { get; set; }
    public decimal? PemakaianListrikKwh { get; set; }

    public RencanaPemanfaatanGas? RencanaPemanfaatanGas { get; set; }
    public string? DeskripsiProsesProduksi { get; set; }

    public decimal? MinEfisiensiDiharapkanPct { get; set; }

    // Surveyor-only — must never render on the customer-facing KK0 printout.
    public decimal? WillingnessToPayUsdMmbtu { get; set; }

    public string? KeteranganLain { get; set; }

    // Live sum of survey_equipment.konversi_ke_gas, stored rather than
    // computed on read because it appears on signed documents. Recomputed on
    // every equipment row edit/delete.
    public decimal JumlahKebutuhanEnergi { get; set; }
}
