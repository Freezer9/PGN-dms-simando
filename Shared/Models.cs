namespace Pgn.Dms.Shared;

public enum SubscriptionStatus
{
    Directory = 0,
    Plotting = 1,
    Prospect = 2,
    Survey = 3,
    A1 = 4,
    PermohonanNOL = 5,
    Disetujui = 6,
    Ditolak = 7,
    // Appended, not inserted: the enum persists as int and existing rows carry 6/7.
    Evaluasi = 8,
    Penerbitan = 9
}

/// <summary>
/// Stage ordering for <see cref="SubscriptionStatus"/>. The enum's numeric values no longer
/// match the workflow order (Evaluasi/Penerbitan were appended), so never compare statuses
/// with &lt; or &gt; — go through this helper.
/// </summary>
public static class SubscriptionStages
{
    /// <summary>The eight workflow stages in order. Disetujui/Ditolak are terminal, not stages.</summary>
    public static readonly SubscriptionStatus[] Order =
    [
        SubscriptionStatus.Directory,
        SubscriptionStatus.Plotting,
        SubscriptionStatus.Prospect,
        SubscriptionStatus.Survey,
        SubscriptionStatus.A1,
        SubscriptionStatus.PermohonanNOL,
        SubscriptionStatus.Evaluasi,
        SubscriptionStatus.Penerbitan
    ];

    /// <summary>Position in <see cref="Order"/>, or -1 for the terminal statuses.</summary>
    public static int IndexOf(SubscriptionStatus status) => Array.IndexOf(Order, status);

    public static bool IsTerminal(SubscriptionStatus status)
        => status is SubscriptionStatus.Disetujui or SubscriptionStatus.Ditolak;

    /// <summary>True when <paramref name="status"/> has reached <paramref name="stage"/> or passed it.</summary>
    public static bool IsAtOrAfter(SubscriptionStatus status, SubscriptionStatus stage)
    {
        if (IsTerminal(status)) return true;
        var a = IndexOf(status);
        var b = IndexOf(stage);
        return a >= 0 && b >= 0 && a >= b;
    }

    /// <summary>True when <paramref name="status"/> has not yet reached <paramref name="stage"/>.</summary>
    public static bool IsBefore(SubscriptionStatus status, SubscriptionStatus stage)
        => !IsAtOrAfter(status, stage);

    public static SubscriptionStatus? Next(SubscriptionStatus status)
    {
        var i = IndexOf(status);
        return i >= 0 && i < Order.Length - 1 ? Order[i + 1] : null;
    }

    public static string Label(SubscriptionStatus status) => status switch
    {
        SubscriptionStatus.Directory => "Direktori",
        SubscriptionStatus.Plotting => "Plotting",
        SubscriptionStatus.Prospect => "Prospek",
        SubscriptionStatus.Survey => "Survei",
        SubscriptionStatus.A1 => "A1",
        SubscriptionStatus.PermohonanNOL => "Permohonan NOL",
        SubscriptionStatus.Evaluasi => "Evaluasi",
        SubscriptionStatus.Penerbitan => "Penerbitan",
        SubscriptionStatus.Disetujui => "Disetujui",
        SubscriptionStatus.Ditolak => "Ditolak",
        _ => status.ToString()
    };
}

public enum ReviewAction
{
    Setuju,
    Tolak,
    Revisi
}

public enum PosisiPelanggan
{
    PengembanganJaringan = 0,
    JalurExisting = 1
}

public enum Kawasan
{
    KawasanIndustri = 0,
    NonKawasanIndustri = 1
}

public enum BasisKontrak
{
    Harian = 0,
    Bulanan = 1,
    Tahunan = 2
}

public enum SkemaHarga
{
    Reguler = 0,
    SiGas = 1,
    Bersyarat = 2
}

public enum StatusBangunan
{
    DalamRencana = 0,
    DalamPembangunan = 1,
    Eksisting = 2,
    ProsesEkspansi = 3
}

public enum Sektor
{
    Komersial = 0,
    Industri = 1,
    Transportasi = 2
}

public enum SignatureMethod
{
    Wet = 0,
    Digital = 1
}

public enum RegistrationType
{
    RegistrasiBaru = 0,
    Amendemen = 1,
    Perpanjangan = 2
}

public enum FeedStatus
{
    Belum = 0,
    DalamProses = 1,
    Selesai = 2
}

public enum StatusRkap
{
    Rkap = 0,
    NonRkap = 1
}

public enum SkemaPembayaran
{
    JaminanPembayaran = 0,
    PembayaranDimuka = 1
}

public enum IssuanceOutcome
{
    Nol = 0,
    Rl = 1
}

public enum MoneyCurrency
{
    Usd = 0,
    Idr = 1
}

public enum VolumeUnit
{
    MMBtu = 0,
    M3 = 1
}

public enum PipeUnit
{
    Inch = 0,
    Mm = 1
}

public enum MasterCategory
{
    Segment = 0,
    IndustryType = 1,
    Country = 2,
    FuelType = 3,
    UnitOfMeasure = 4,
    MeterSize = 5,
    MrsSpec = 6,
    ReferenceDocument = 7,
    ReasonCategory = 8
}

public class UserInfo
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AreaId { get; set; }
    public string? AreaName { get; set; }
}

public class RegionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<AreaDto> Areas { get; set; } = [];
}

public class AreaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
}

public class SubscriptionDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string Address { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public SubscriptionStatus Status { get; set; }
    public int AreaId { get; set; }
    public string? AreaName { get; set; }
    public string? RegionName { get; set; }
    public string CreatedById { get; set; } = "";
    public string? CreatedByName { get; set; }
    public int CurrentReviewerIndex { get; set; } = -1;
    public string ReviewerIds { get; set; } = "";
    public bool SignedOff { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SubmissionRecordDto> Submissions { get; set; } = [];
    public List<ReviewStepDto> ReviewSteps { get; set; } = [];
    public ResumeEvaluasiDto? ResumeEvaluasi { get; set; }
    public List<ActivityLogDto> ActivityLogs { get; set; } = [];
}

public class SubmissionRecordDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public SubscriptionStatus Stage { get; set; }
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string UploadedById { get; set; } = "";
    public string? UploadedByName { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ReviewStepDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string ReviewerId { get; set; } = "";
    public string? ReviewerName { get; set; }
    public int StepOrder { get; set; }
    public ReviewAction? Action { get; set; }
    public string Comment { get; set; } = "";
    public DateTime? ReviewedAt { get; set; }
}

public class ResumeEvaluasiDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string Content { get; set; } = "";
    public string CreatedById { get; set; } = "";
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public int? SubscriptionId { get; set; }
    public string ActorName { get; set; } = "";
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime At { get; set; }
}

public class CreateSubscriptionRequest
{
    public string CompanyName { get; set; } = "";
    public string Address { get; set; } = "";
    public int AreaId { get; set; }
    /// <summary>Map pin. Docs makes this mandatory at stage 1.</summary>
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class SubmitReviewRequest
{
    public ReviewAction Action { get; set; }
    public string Comment { get; set; } = "";
}

public class SaveResumeRequest
{
    public string Content { get; set; } = "";
}

public class CreateUserRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
    public int? AreaId { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public UserInfo User { get; set; } = new();
}

public class AssignReviewersRequest
{
    public string[] ReviewerIds { get; set; } = [];
}

// ── Stage 2: Plotting ───────────────────────────────────────────────────────

public class PlottingDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string? SalesUserId { get; set; }
    public string? SalesUserName { get; set; }
    public PosisiPelanggan? PosisiPelanggan { get; set; }
    public Kawasan? Kawasan { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gate 2→3: all three fields must be set.</summary>
    public bool IsComplete => !string.IsNullOrWhiteSpace(SalesUserId)
        && PosisiPelanggan.HasValue && Kawasan.HasValue;
}

public class SavePlottingRequest
{
    public string? SalesUserId { get; set; }
    public PosisiPelanggan? PosisiPelanggan { get; set; }
    public Kawasan? Kawasan { get; set; }
}

// ── Stage 3: Prospek (PIC contacts) ─────────────────────────────────────────

public class CompanyContactDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string Nama { get; set; } = "";
    public string Jabatan { get; set; } = "";
    public string Email { get; set; } = "";
    public string NoHp { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string Instagram { get; set; } = "";
    public string Facebook { get; set; } = "";
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class SaveContactRequest
{
    public int? Id { get; set; }
    public string Nama { get; set; } = "";
    public string Jabatan { get; set; } = "";
    public string Email { get; set; } = "";
    public string NoHp { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string Instagram { get; set; } = "";
    public string Facebook { get; set; } = "";
    public bool IsPrimary { get; set; }
}

// ── Stage 4: Survei (KK0) ───────────────────────────────────────────────────

public class SurveyDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime? TanggalSurvey { get; set; }
    public string? SurveyorUserId { get; set; }
    public string? SurveyorName { get; set; }

    // Operasi
    public int? JumlahKaryawan { get; set; }
    public int? JumlahShift { get; set; }
    public double? JamKerjaPerHari { get; set; }
    public int? HariPerMinggu { get; set; }

    // Kebutuhan energi — CSV of listrik|steam|panas|dingin|lainnya
    public string KebutuhanEnergi { get; set; } = "";
    public double? KapasitasNilai { get; set; }
    public string KapasitasUnit { get; set; } = "";
    public double? PemakaianNilai { get; set; }
    public string PemakaianUnit { get; set; } = "";
    public double? JumlahKebutuhanEnergi { get; set; }

    // Pipa terdekat
    public double? PipaTerdekatJarakM { get; set; }
    public double? PipaTerdekatDiameter { get; set; }
    public double? PipaTerdekatTekanan { get; set; }

    // Bahan bakar eksisting
    public string BahanBakarEksisting { get; set; } = "";
    public string NamaPemasok { get; set; } = "";
    public double? KapasitasListrik { get; set; }
    public double? PemakaianListrik { get; set; }

    // Narasi
    public string RencanaPemanfaatanGas { get; set; } = "";
    public string DeskripsiProsesProduksi { get; set; } = "";
    public string KeteranganLain { get; set; } = "";

    // Beban puncak — dua jendela waktu
    public string BebanPuncak1Mulai { get; set; } = "";
    public string BebanPuncak1Selesai { get; set; } = "";
    public string BebanPuncak2Mulai { get; set; } = "";
    public string BebanPuncak2Selesai { get; set; } = "";

    public double? MinEfisiensiDiharapkanPct { get; set; }
    public double? WillingnessToPayUsdMmbtu { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<SurveyProductDto> Products { get; set; } = [];
    public List<SurveyRawMaterialDto> RawMaterials { get; set; } = [];
    public List<SurveyMarketDto> Markets { get; set; } = [];
    public List<SurveyEquipmentDto> Equipment { get; set; } = [];
}

public class SurveyProductDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public double? Kapasitas { get; set; }
    public string Unit { get; set; } = "";
    public int SortOrder { get; set; }
}

public class SurveyRawMaterialDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public bool IsImpor { get; set; }
    public string NegaraAsal { get; set; } = "";
    public int SortOrder { get; set; }
}

public class SurveyMarketDto
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public bool IsEkspor { get; set; }
    public double? PersentasePct { get; set; }
    public int SortOrder { get; set; }
}

public class SurveyEquipmentDto
{
    public int Id { get; set; }
    public string Jenis { get; set; } = "";
    public double? Kapasitas { get; set; }
    public double? JamPerHari { get; set; }
    public int? HariPerMinggu { get; set; }
    public string BahanBakar { get; set; } = "";
    public double? HargaBahanBakar { get; set; }
    public double? KonsumsiPerBulan { get; set; }
    /// <summary>Typed by the surveyor, never computed — see Docs/data-model.md.</summary>
    public double? KonversiKeGas { get; set; }
    public int SortOrder { get; set; }
}

// ── Stage 5: A1 (Registrasi) ────────────────────────────────────────────────

public class A1RegistrationDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime? TanggalRegistrasi { get; set; }
    public string NamaPenanggungJawab { get; set; } = "";
    public string JabatanPenanggungJawab { get; set; } = "";
    public string BulanDimulai { get; set; } = "";

    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }
    public int? SegmentId { get; set; }
    public string? SegmentName { get; set; }
    public string KodeHarga { get; set; } = "";
    public double? HargaNilai { get; set; }
    public MoneyCurrency? HargaCurrency { get; set; }
    public VolumeUnit? HargaUnit { get; set; }

    public double? CapexAwal { get; set; }
    public bool MomSigasTersedia { get; set; }
    public StatusBangunan? StatusBangunan { get; set; }
    public Sektor? Sektor { get; set; }
    public string ProduksiUtama { get; set; } = "";
    public string JenisPeralatanGas { get; set; } = "";
    public double? TekananOperasiBarg { get; set; }
    public SignatureMethod? SignatureMethod { get; set; }

    public DateTime UpdatedAt { get; set; }
    public List<A1UsagePeriodDto> Periods { get; set; } = [];
}

public class A1UsagePeriodDto
{
    public int Id { get; set; }
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }
}

// ── Stage 6: Permohonan NOL ─────────────────────────────────────────────────

public class NolRequestDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string NomorNotaDinas { get; set; } = "";
    public RegistrationType? RegistrationType { get; set; }
    public bool SamaDenganA1 { get; set; }
    public string BulanDimulai { get; set; } = "";

    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }
    public int? SegmentId { get; set; }
    public string? SegmentName { get; set; }
    public string KodeHarga { get; set; } = "";
    public double? HargaNilai { get; set; }
    public MoneyCurrency? HargaCurrency { get; set; }
    public VolumeUnit? HargaUnit { get; set; }

    public string AlasanKontrakBersyarat { get; set; } = "";
    public string NamaPimpinanPerusahaan { get; set; } = "";
    public string JangkaWaktuKontrak { get; set; } = "";
    /// <summary>Lampiran 17 narrative.</summary>
    public string Lampiran17 { get; set; } = "";

    public double? CapexPreGr3 { get; set; }
    public double? BiayaPenyambunganReguler { get; set; }
    public double? BiayaPenyambunganExtra { get; set; }
    /// <summary>Derived: reguler + extra.</summary>
    public double BiayaPenyambunganJumlah
        => (BiayaPenyambunganReguler ?? 0) + (BiayaPenyambunganExtra ?? 0);

    public DateTime? SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<NolRequestPeriodDto> Periods { get; set; } = [];
    public List<NolRequestReferenceDto> References { get; set; } = [];
}

public class NolRequestPeriodDto
{
    public int Id { get; set; }
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }
}

public class NolRequestReferenceDto
{
    public int Id { get; set; }
    public string Judul { get; set; } = "";
    public string Nomor { get; set; } = "";
    public DateTime? Tanggal { get; set; }
    public int SortOrder { get; set; }
}

// ── Stage 7: Evaluasi ───────────────────────────────────────────────────────

public class NolEvaluationDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }

    public FeedStatus? FeedStatus { get; set; }
    public DateTime? FeedCompletedAt { get; set; }

    public double? CapexFinal { get; set; }
    public double? PipaIndukPanjang { get; set; }
    public double? PipaIndukDiameter { get; set; }
    public PipeUnit? PipaIndukUnit { get; set; }
    public double? PipaServicePanjang { get; set; }
    public double? PipaServiceDiameter { get; set; }
    public PipeUnit? PipaServiceUnit { get; set; }

    public int? MrsSpecId { get; set; }
    public string? MrsSpecName { get; set; }
    public int? MeterSizeId { get; set; }
    public string? MeterSizeName { get; set; }
    public double? Tekanan { get; set; }
    public double? MaksFlowrate { get; set; }
    public double? MaksKapasitasMeterM3Jam { get; set; }
    public int? DurasiPelaksanaanBulan { get; set; }

    public StatusRkap? StatusRkap { get; set; }
    public SkemaPembayaran? SkemaPembayaran { get; set; }
    public string JaminanStatus { get; set; } = "";
    public string JaminanJenis { get; set; } = "";
    public string JaminanPenerbit { get; set; } = "";
    public DateTime? JaminanMasaBerlaku { get; set; }

    public double? KetersediaanPasokanBbtud { get; set; }
    public string AnalisisKomersial { get; set; } = "";
    public string AnalisisKompetitor { get; set; } = "";
    public double? RadiusKompetitorKm { get; set; }
    public string Kesimpulan { get; set; } = "";

    public string? EvaluatedById { get; set; }
    public string? EvaluatedByName { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<NolEvaluationScenarioDto> Scenarios { get; set; } = [];
}

public class NolEvaluationScenarioDto
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public double? IrrPct { get; set; }
    public double? Npv { get; set; }
    public double? PaybackYears { get; set; }
    public string HasilAnalisis { get; set; } = "";
    public int SortOrder { get; set; }
}

// ── Stage 8: Penerbitan ─────────────────────────────────────────────────────

public class NolIssuanceDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public IssuanceOutcome Outcome { get; set; }
    public string NomorNotaDinas { get; set; } = "";
    public DateTime? BerlakuSejak { get; set; }
    public DateTime? BerlakuSampai { get; set; }
    public string? SignedById { get; set; }
    public string? SignedByName { get; set; }
    public DateTime? SignedAt { get; set; }
    public string Catatan { get; set; } = "";

    public List<NolIssuanceTermDto> ApprovedTerms { get; set; } = [];
    public List<NolIssuanceConditionDto> Conditions { get; set; } = [];
}

public class NolIssuanceTermDto
{
    public int Id { get; set; }
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }
}

public class NolIssuanceConditionDto
{
    public int Id { get; set; }
    public string Isi { get; set; } = "";
    public int SortOrder { get; set; }
}

public class IssueNolRequest
{
    public IssuanceOutcome Outcome { get; set; }
    public string NomorNotaDinas { get; set; } = "";
    public DateTime? BerlakuSejak { get; set; }
    public DateTime? BerlakuSampai { get; set; }
    public string Catatan { get; set; } = "";
    public List<NolIssuanceTermDto> ApprovedTerms { get; set; } = [];
    public List<NolIssuanceConditionDto> Conditions { get; set; } = [];
}

// ── Master data ─────────────────────────────────────────────────────────────

public class MasterDataEntryDto
{
    public int Id { get; set; }
    public MasterCategory Category { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    /// <summary>Category-specific extras as JSON (e.g. meter-size flow/pressure).</summary>
    public string AttributesJson { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveMasterDataRequest
{
    public MasterCategory Category { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string AttributesJson { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── Break glass ─────────────────────────────────────────────────────────────

public class BreakGlassGrantDto
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public string? CompanyName { get; set; }
    public string GrantedToUserId { get; set; } = "";
    public string? GrantedToName { get; set; }
    public string GrantedById { get; set; } = "";
    public string? GrantedByName { get; set; }
    public string Reason { get; set; } = "";
    public DateTime GrantedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;
}

public class GrantBreakGlassRequest
{
    public int SubscriptionId { get; set; }
    public string GrantedToUserId { get; set; } = "";
    public string Reason { get; set; } = "";
}

// ── Workflow gates ──────────────────────────────────────────────────────────

/// <summary>Result of an advance attempt. <see cref="Reason"/> names the blocking gate.</summary>
public class AdvanceResult
{
    public bool Ok { get; set; }
    public string? Reason { get; set; }
    public SubscriptionStatus? NewStatus { get; set; }
}

public class UpdateLocationRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
