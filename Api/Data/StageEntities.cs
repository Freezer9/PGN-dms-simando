using Pgn.Dms.Shared;

namespace Pgn.Dms.Api.Data;

// Stage satellite tables. One aggregate per workflow stage, hanging off Subscription.
// Convention matches Entities.cs: public props, `= default!` navigations, a ToDto().

// ── Stage 2: Plotting ───────────────────────────────────────────────────────

public class Plotting
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public string? SalesUserId { get; set; }
    public ApplicationUser? SalesUser { get; set; }
    public PosisiPelanggan? PosisiPelanggan { get; set; }
    public Kawasan? Kawasan { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public PlottingDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        SalesUserId = SalesUserId,
        SalesUserName = SalesUser?.FullName,
        PosisiPelanggan = PosisiPelanggan,
        Kawasan = Kawasan,
        UpdatedAt = UpdatedAt
    };
}

// ── Stage 3: Prospek ────────────────────────────────────────────────────────

public class CompanyContact
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public required string Nama { get; set; }
    public required string Jabatan { get; set; }
    public string Email { get; set; } = "";
    public string NoHp { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string Instagram { get; set; } = "";
    public string Facebook { get; set; } = "";
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public CompanyContactDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        Nama = Nama,
        Jabatan = Jabatan,
        Email = Email,
        NoHp = NoHp,
        LinkedIn = LinkedIn,
        Instagram = Instagram,
        Facebook = Facebook,
        IsPrimary = IsPrimary,
        SortOrder = SortOrder
    };
}

// ── Stage 4: Survei (KK0) ───────────────────────────────────────────────────

public class Survey
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

    public DateTime? TanggalSurvey { get; set; }
    public string? SurveyorUserId { get; set; }
    public ApplicationUser? SurveyorUser { get; set; }

    public int? JumlahKaryawan { get; set; }
    public int? JumlahShift { get; set; }
    public double? JamKerjaPerHari { get; set; }
    public int? HariPerMinggu { get; set; }

    public string KebutuhanEnergi { get; set; } = "";
    public double? KapasitasNilai { get; set; }
    public string KapasitasUnit { get; set; } = "";
    public double? PemakaianNilai { get; set; }
    public string PemakaianUnit { get; set; } = "";
    public double? JumlahKebutuhanEnergi { get; set; }

    public double? PipaTerdekatJarakM { get; set; }
    public double? PipaTerdekatDiameter { get; set; }
    public double? PipaTerdekatTekanan { get; set; }

    public string BahanBakarEksisting { get; set; } = "";
    public string NamaPemasok { get; set; } = "";
    public double? KapasitasListrik { get; set; }
    public double? PemakaianListrik { get; set; }

    public string RencanaPemanfaatanGas { get; set; } = "";
    public string DeskripsiProsesProduksi { get; set; } = "";
    public string KeteranganLain { get; set; } = "";

    public string BebanPuncak1Mulai { get; set; } = "";
    public string BebanPuncak1Selesai { get; set; } = "";
    public string BebanPuncak2Mulai { get; set; } = "";
    public string BebanPuncak2Selesai { get; set; } = "";

    public double? MinEfisiensiDiharapkanPct { get; set; }
    public double? WillingnessToPayUsdMmbtu { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<SurveyProduct> Products { get; set; } = [];
    public List<SurveyRawMaterial> RawMaterials { get; set; } = [];
    public List<SurveyMarket> Markets { get; set; } = [];
    public List<SurveyEquipment> Equipment { get; set; } = [];

    public SurveyDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        TanggalSurvey = TanggalSurvey,
        SurveyorUserId = SurveyorUserId,
        SurveyorName = SurveyorUser?.FullName,
        JumlahKaryawan = JumlahKaryawan,
        JumlahShift = JumlahShift,
        JamKerjaPerHari = JamKerjaPerHari,
        HariPerMinggu = HariPerMinggu,
        KebutuhanEnergi = KebutuhanEnergi,
        KapasitasNilai = KapasitasNilai,
        KapasitasUnit = KapasitasUnit,
        PemakaianNilai = PemakaianNilai,
        PemakaianUnit = PemakaianUnit,
        JumlahKebutuhanEnergi = JumlahKebutuhanEnergi,
        PipaTerdekatJarakM = PipaTerdekatJarakM,
        PipaTerdekatDiameter = PipaTerdekatDiameter,
        PipaTerdekatTekanan = PipaTerdekatTekanan,
        BahanBakarEksisting = BahanBakarEksisting,
        NamaPemasok = NamaPemasok,
        KapasitasListrik = KapasitasListrik,
        PemakaianListrik = PemakaianListrik,
        RencanaPemanfaatanGas = RencanaPemanfaatanGas,
        DeskripsiProsesProduksi = DeskripsiProsesProduksi,
        KeteranganLain = KeteranganLain,
        BebanPuncak1Mulai = BebanPuncak1Mulai,
        BebanPuncak1Selesai = BebanPuncak1Selesai,
        BebanPuncak2Mulai = BebanPuncak2Mulai,
        BebanPuncak2Selesai = BebanPuncak2Selesai,
        MinEfisiensiDiharapkanPct = MinEfisiensiDiharapkanPct,
        WillingnessToPayUsdMmbtu = WillingnessToPayUsdMmbtu,
        UpdatedAt = UpdatedAt,
        Products = Products.OrderBy(p => p.SortOrder).Select(p => p.ToDto()).ToList(),
        RawMaterials = RawMaterials.OrderBy(r => r.SortOrder).Select(r => r.ToDto()).ToList(),
        Markets = Markets.OrderBy(m => m.SortOrder).Select(m => m.ToDto()).ToList(),
        Equipment = Equipment.OrderBy(e => e.SortOrder).Select(e => e.ToDto()).ToList()
    };
}

public class SurveyProduct
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public Survey Survey { get; set; } = default!;
    public string Nama { get; set; } = "";
    public double? Kapasitas { get; set; }
    public string Unit { get; set; } = "";
    public int SortOrder { get; set; }

    public SurveyProductDto ToDto() => new()
    {
        Id = Id, Nama = Nama, Kapasitas = Kapasitas, Unit = Unit, SortOrder = SortOrder
    };
}

public class SurveyRawMaterial
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public Survey Survey { get; set; } = default!;
    public string Nama { get; set; } = "";
    public bool IsImpor { get; set; }
    public string NegaraAsal { get; set; } = "";
    public int SortOrder { get; set; }

    public SurveyRawMaterialDto ToDto() => new()
    {
        Id = Id, Nama = Nama, IsImpor = IsImpor, NegaraAsal = NegaraAsal, SortOrder = SortOrder
    };
}

public class SurveyMarket
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public Survey Survey { get; set; } = default!;
    public string Nama { get; set; } = "";
    public bool IsEkspor { get; set; }
    public double? PersentasePct { get; set; }
    public int SortOrder { get; set; }

    public SurveyMarketDto ToDto() => new()
    {
        Id = Id, Nama = Nama, IsEkspor = IsEkspor, PersentasePct = PersentasePct, SortOrder = SortOrder
    };
}

public class SurveyEquipment
{
    public int Id { get; set; }
    public int SurveyId { get; set; }
    public Survey Survey { get; set; } = default!;
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

    public SurveyEquipmentDto ToDto() => new()
    {
        Id = Id, Jenis = Jenis, Kapasitas = Kapasitas, JamPerHari = JamPerHari,
        HariPerMinggu = HariPerMinggu, BahanBakar = BahanBakar, HargaBahanBakar = HargaBahanBakar,
        KonsumsiPerBulan = KonsumsiPerBulan, KonversiKeGas = KonversiKeGas, SortOrder = SortOrder
    };
}

// ── Stage 5: A1 ─────────────────────────────────────────────────────────────

public class A1Registration
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

    public DateTime? TanggalRegistrasi { get; set; }
    public string NamaPenanggungJawab { get; set; } = "";
    public string JabatanPenanggungJawab { get; set; } = "";
    public string BulanDimulai { get; set; } = "";

    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }
    public int? SegmentId { get; set; }
    public MasterDataEntry? Segment { get; set; }
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

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<A1UsagePeriod> Periods { get; set; } = [];

    public A1RegistrationDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        TanggalRegistrasi = TanggalRegistrasi,
        NamaPenanggungJawab = NamaPenanggungJawab,
        JabatanPenanggungJawab = JabatanPenanggungJawab,
        BulanDimulai = BulanDimulai,
        BasisKontrak = BasisKontrak,
        SkemaHarga = SkemaHarga,
        SegmentId = SegmentId,
        SegmentName = Segment?.Name,
        KodeHarga = KodeHarga,
        HargaNilai = HargaNilai,
        HargaCurrency = HargaCurrency,
        HargaUnit = HargaUnit,
        CapexAwal = CapexAwal,
        MomSigasTersedia = MomSigasTersedia,
        StatusBangunan = StatusBangunan,
        Sektor = Sektor,
        ProduksiUtama = ProduksiUtama,
        JenisPeralatanGas = JenisPeralatanGas,
        TekananOperasiBarg = TekananOperasiBarg,
        SignatureMethod = SignatureMethod,
        UpdatedAt = UpdatedAt,
        Periods = Periods.OrderBy(p => p.SortOrder).Select(p => p.ToDto()).ToList()
    };
}

public class A1UsagePeriod
{
    public int Id { get; set; }
    public int A1RegistrationId { get; set; }
    public A1Registration A1Registration { get; set; } = default!;
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }

    public A1UsagePeriodDto ToDto() => new()
    {
        Id = Id, PeriodeMulai = PeriodeMulai, PeriodeSelesai = PeriodeSelesai,
        RataRata = RataRata, Minimum = Minimum, Maksimum = Maksimum, SortOrder = SortOrder
    };
}

// ── Stage 6: Permohonan NOL ─────────────────────────────────────────────────

public class NolRequest
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

    public string NomorNotaDinas { get; set; } = "";
    public RegistrationType? RegistrationType { get; set; }
    public bool SamaDenganA1 { get; set; }
    public string BulanDimulai { get; set; } = "";

    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }
    public int? SegmentId { get; set; }
    public MasterDataEntry? Segment { get; set; }
    public string KodeHarga { get; set; } = "";
    public double? HargaNilai { get; set; }
    public MoneyCurrency? HargaCurrency { get; set; }
    public VolumeUnit? HargaUnit { get; set; }

    public string AlasanKontrakBersyarat { get; set; } = "";
    public string NamaPimpinanPerusahaan { get; set; } = "";
    public string JangkaWaktuKontrak { get; set; } = "";
    public string Lampiran17 { get; set; } = "";

    public double? CapexPreGr3 { get; set; }
    public double? BiayaPenyambunganReguler { get; set; }
    public double? BiayaPenyambunganExtra { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<NolRequestPeriod> Periods { get; set; } = [];
    public List<NolRequestReference> References { get; set; } = [];

    public NolRequestDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        NomorNotaDinas = NomorNotaDinas,
        RegistrationType = RegistrationType,
        SamaDenganA1 = SamaDenganA1,
        BulanDimulai = BulanDimulai,
        BasisKontrak = BasisKontrak,
        SkemaHarga = SkemaHarga,
        SegmentId = SegmentId,
        SegmentName = Segment?.Name,
        KodeHarga = KodeHarga,
        HargaNilai = HargaNilai,
        HargaCurrency = HargaCurrency,
        HargaUnit = HargaUnit,
        AlasanKontrakBersyarat = AlasanKontrakBersyarat,
        NamaPimpinanPerusahaan = NamaPimpinanPerusahaan,
        JangkaWaktuKontrak = JangkaWaktuKontrak,
        Lampiran17 = Lampiran17,
        CapexPreGr3 = CapexPreGr3,
        BiayaPenyambunganReguler = BiayaPenyambunganReguler,
        BiayaPenyambunganExtra = BiayaPenyambunganExtra,
        SubmittedAt = SubmittedAt,
        UpdatedAt = UpdatedAt,
        Periods = Periods.OrderBy(p => p.SortOrder).Select(p => p.ToDto()).ToList(),
        References = References.OrderBy(r => r.SortOrder).Select(r => r.ToDto()).ToList()
    };
}

public class NolRequestPeriod
{
    public int Id { get; set; }
    public int NolRequestId { get; set; }
    public NolRequest NolRequest { get; set; } = default!;
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }

    public NolRequestPeriodDto ToDto() => new()
    {
        Id = Id, PeriodeMulai = PeriodeMulai, PeriodeSelesai = PeriodeSelesai,
        RataRata = RataRata, Minimum = Minimum, Maksimum = Maksimum, SortOrder = SortOrder
    };
}

public class NolRequestReference
{
    public int Id { get; set; }
    public int NolRequestId { get; set; }
    public NolRequest NolRequest { get; set; } = default!;
    public string Judul { get; set; } = "";
    public string Nomor { get; set; } = "";
    public DateTime? Tanggal { get; set; }
    public int SortOrder { get; set; }

    public NolRequestReferenceDto ToDto() => new()
    {
        Id = Id, Judul = Judul, Nomor = Nomor, Tanggal = Tanggal, SortOrder = SortOrder
    };
}

// ── Stage 7: Evaluasi ───────────────────────────────────────────────────────

public class NolEvaluation
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

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
    public MasterDataEntry? MrsSpec { get; set; }
    public int? MeterSizeId { get; set; }
    public MasterDataEntry? MeterSize { get; set; }
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
    public ApplicationUser? EvaluatedBy { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<NolEvaluationScenario> Scenarios { get; set; } = [];

    public NolEvaluationDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        FeedStatus = FeedStatus,
        FeedCompletedAt = FeedCompletedAt,
        CapexFinal = CapexFinal,
        PipaIndukPanjang = PipaIndukPanjang,
        PipaIndukDiameter = PipaIndukDiameter,
        PipaIndukUnit = PipaIndukUnit,
        PipaServicePanjang = PipaServicePanjang,
        PipaServiceDiameter = PipaServiceDiameter,
        PipaServiceUnit = PipaServiceUnit,
        MrsSpecId = MrsSpecId,
        MrsSpecName = MrsSpec?.Name,
        MeterSizeId = MeterSizeId,
        MeterSizeName = MeterSize?.Name,
        Tekanan = Tekanan,
        MaksFlowrate = MaksFlowrate,
        MaksKapasitasMeterM3Jam = MaksKapasitasMeterM3Jam,
        DurasiPelaksanaanBulan = DurasiPelaksanaanBulan,
        StatusRkap = StatusRkap,
        SkemaPembayaran = SkemaPembayaran,
        JaminanStatus = JaminanStatus,
        JaminanJenis = JaminanJenis,
        JaminanPenerbit = JaminanPenerbit,
        JaminanMasaBerlaku = JaminanMasaBerlaku,
        KetersediaanPasokanBbtud = KetersediaanPasokanBbtud,
        AnalisisKomersial = AnalisisKomersial,
        AnalisisKompetitor = AnalisisKompetitor,
        RadiusKompetitorKm = RadiusKompetitorKm,
        Kesimpulan = Kesimpulan,
        EvaluatedById = EvaluatedById,
        EvaluatedByName = EvaluatedBy?.FullName,
        EvaluatedAt = EvaluatedAt,
        UpdatedAt = UpdatedAt,
        Scenarios = Scenarios.OrderBy(s => s.SortOrder).Select(s => s.ToDto()).ToList()
    };
}

public class NolEvaluationScenario
{
    public int Id { get; set; }
    public int NolEvaluationId { get; set; }
    public NolEvaluation NolEvaluation { get; set; } = default!;
    public string Label { get; set; } = "";
    public double? IrrPct { get; set; }
    public double? Npv { get; set; }
    public double? PaybackYears { get; set; }
    public string HasilAnalisis { get; set; } = "";
    public int SortOrder { get; set; }

    public NolEvaluationScenarioDto ToDto() => new()
    {
        Id = Id, Label = Label, IrrPct = IrrPct, Npv = Npv,
        PaybackYears = PaybackYears, HasilAnalisis = HasilAnalisis, SortOrder = SortOrder
    };
}

// ── Stage 8: Penerbitan ─────────────────────────────────────────────────────

public class NolIssuance
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;

    public IssuanceOutcome Outcome { get; set; }
    public string NomorNotaDinas { get; set; } = "";
    public DateTime? BerlakuSejak { get; set; }
    public DateTime? BerlakuSampai { get; set; }
    public string? SignedById { get; set; }
    public ApplicationUser? SignedBy { get; set; }
    public DateTime? SignedAt { get; set; }
    public string Catatan { get; set; } = "";

    public List<NolIssuanceTerm> ApprovedTerms { get; set; } = [];
    public List<NolIssuanceCondition> Conditions { get; set; } = [];

    public NolIssuanceDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        Outcome = Outcome,
        NomorNotaDinas = NomorNotaDinas,
        BerlakuSejak = BerlakuSejak,
        BerlakuSampai = BerlakuSampai,
        SignedById = SignedById,
        SignedByName = SignedBy?.FullName,
        SignedAt = SignedAt,
        Catatan = Catatan,
        ApprovedTerms = ApprovedTerms.OrderBy(t => t.SortOrder).Select(t => t.ToDto()).ToList(),
        Conditions = Conditions.OrderBy(c => c.SortOrder).Select(c => c.ToDto()).ToList()
    };
}

public class NolIssuanceTerm
{
    public int Id { get; set; }
    public int NolIssuanceId { get; set; }
    public NolIssuance NolIssuance { get; set; } = default!;
    public DateTime? PeriodeMulai { get; set; }
    public DateTime? PeriodeSelesai { get; set; }
    public double? RataRata { get; set; }
    public double? Minimum { get; set; }
    public double? Maksimum { get; set; }
    public int SortOrder { get; set; }

    public NolIssuanceTermDto ToDto() => new()
    {
        Id = Id, PeriodeMulai = PeriodeMulai, PeriodeSelesai = PeriodeSelesai,
        RataRata = RataRata, Minimum = Minimum, Maksimum = Maksimum, SortOrder = SortOrder
    };
}

public class NolIssuanceCondition
{
    public int Id { get; set; }
    public int NolIssuanceId { get; set; }
    public NolIssuance NolIssuance { get; set; } = default!;
    public string Isi { get; set; } = "";
    public int SortOrder { get; set; }

    public NolIssuanceConditionDto ToDto() => new()
    {
        Id = Id, Isi = Isi, SortOrder = SortOrder
    };
}

// ── Master data ─────────────────────────────────────────────────────────────

/// <summary>
/// One table for every reference list (segment, G-size, MRS spec, units, …), keyed by
/// <see cref="Category"/>. Docs/data-model.md models these as 11 separate tables; a single
/// table with a discriminator plus a JSON <see cref="AttributesJson"/> bag serves the same
/// UI with one controller. Split it out if per-table columns or FKs become necessary.
/// </summary>
public class MasterDataEntry
{
    public int Id { get; set; }
    public MasterCategory Category { get; set; }
    public string Code { get; set; } = "";
    public required string Name { get; set; }
    public string Description { get; set; } = "";
    public string AttributesJson { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public MasterDataEntryDto ToDto() => new()
    {
        Id = Id, Category = Category, Code = Code, Name = Name, Description = Description,
        AttributesJson = AttributesJson, SortOrder = SortOrder, IsActive = IsActive
    };
}

// ── Break glass ─────────────────────────────────────────────────────────────

public class BreakGlassGrant
{
    public int Id { get; set; }
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = default!;
    public required string GrantedToUserId { get; set; }
    public ApplicationUser GrantedToUser { get; set; } = default!;
    public required string GrantedById { get; set; }
    public ApplicationUser GrantedBy { get; set; } = default!;
    public required string Reason { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public BreakGlassGrantDto ToDto() => new()
    {
        Id = Id,
        SubscriptionId = SubscriptionId,
        CompanyName = Subscription?.CompanyName,
        GrantedToUserId = GrantedToUserId,
        GrantedToName = GrantedToUser?.FullName,
        GrantedById = GrantedById,
        GrantedByName = GrantedBy?.FullName,
        Reason = Reason,
        GrantedAt = GrantedAt,
        ExpiresAt = ExpiresAt,
        RevokedAt = RevokedAt
    };
}
