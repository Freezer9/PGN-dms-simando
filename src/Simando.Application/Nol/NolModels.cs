using Simando.Domain.Nol;
using Simando.Domain.Registration;

namespace Simando.Application.Nol;

public sealed record NolRequestPeriodDetail(
    Guid Id,
    DateOnly PeriodeMulai,
    DateOnly PeriodeSelesai,
    decimal RataRata,
    decimal KontrakMinimum,
    decimal KontrakMaksimum,
    short SortOrder
);

public sealed record NolRequestDailyDetail(
    Guid Id,
    DayOfWeek Hari,
    decimal Min,
    decimal Max
);

public sealed record NolRequestDetail(
    Guid CompanyId,
    string? NomorNotaDinas,
    RegistrationType RegistrationType,
    bool SamaDenganA1,
    DateOnly? BulanDimulai,
    BasisKontrak? BasisKontrak,
    SkemaHarga? SkemaHarga,
    Guid? SegmentId,
    string? KodeHarga,
    decimal? HargaNilai,
    HargaCurrency? HargaCurrency,
    HargaUnit? HargaUnit,
    string? AlasanKontrakBersyarat,
    string? NamaPimpinanPerusahaan,
    string? JangkaWaktuKontrak,
    decimal? CapexPreGr3,
    decimal? BiayaPenyambunganReguler,
    decimal? BiayaPenyambunganExtra,
    decimal BiayaPenyambunganJumlah,
    Guid? WorkflowInstanceId,
    DateTimeOffset? SubmittedAt,
    IReadOnlyList<NolRequestPeriodDetail> Periods,
    IReadOnlyList<NolRequestDailyDetail> DailyBasisRows,
    IReadOnlyList<Guid> ReferenceDocumentIds
);

public sealed record SaveNolRequestRequest(
    string? NomorNotaDinas,
    RegistrationType RegistrationType,
    bool SamaDenganA1,
    DateOnly? BulanDimulai,
    BasisKontrak? BasisKontrak,
    SkemaHarga? SkemaHarga,
    Guid? SegmentId,
    string? KodeHarga,
    decimal? HargaNilai,
    HargaCurrency? HargaCurrency,
    HargaUnit? HargaUnit,
    string? AlasanKontrakBersyarat,
    string? NamaPimpinanPerusahaan,
    string? JangkaWaktuKontrak,
    decimal? CapexPreGr3,
    decimal? BiayaPenyambunganReguler,
    decimal? BiayaPenyambunganExtra,
    IReadOnlyList<NolRequestPeriodDetail> Periods,
    IReadOnlyList<NolRequestDailyDetail> DailyBasisRows,
    IReadOnlyList<Guid> ReferenceDocumentIds
);

public sealed record NolEvaluationScenarioDetail(
    Guid Id,
    string Label,
    decimal? IrrPct,
    decimal? Npv,
    decimal? PaybackYears,
    string? HasilAnalisis
);

public sealed record NolEvaluationDetail(
    Guid NolRequestId,
    FeedStatus FeedStatus,
    DateOnly? FeedCompletedAt,
    decimal? CapexFinal,
    decimal? PipaIndukPanjangM,
    decimal? PipaIndukDiameter,
    DiameterUnit? PipaIndukDiameterUnit,
    decimal? PipaServicePanjangM,
    decimal? PipaServiceDiameter,
    DiameterUnit? PipaServiceDiameterUnit,
    string? SpesifikasiMrs,
    string? GSize,
    decimal? Tekanan,
    decimal? MaksFlowrate,
    decimal? MaksKapasitasMeterM3Jam,
    short? DurasiPelaksanaanBulan,
    StatusRkap? StatusRkap,
    SkemaPembayaran? SkemaPembayaran,
    string? JaminanStatus,
    string? JaminanJenis,
    string? JaminanMasaBerlaku,
    string? JaminanPenerbit,
    decimal? KetersediaanPasokanBbtud,
    string? AnalisisKomersial,
    string? AnalisisKompetitor,
    string? Kesimpulan,
    decimal? RadiusKompetitorKm,
    Guid? EvaluatedBy,
    DateTimeOffset? EvaluatedAt,
    IReadOnlyList<NolEvaluationScenarioDetail> Scenarios
);

public sealed record SaveNolEvaluationRequest(
    FeedStatus FeedStatus,
    DateOnly? FeedCompletedAt,
    decimal? CapexFinal,
    decimal? PipaIndukPanjangM,
    decimal? PipaIndukDiameter,
    DiameterUnit? PipaIndukDiameterUnit,
    decimal? PipaServicePanjangM,
    decimal? PipaServiceDiameter,
    DiameterUnit? PipaServiceDiameterUnit,
    string? SpesifikasiMrs,
    string? GSize,
    decimal? Tekanan,
    decimal? MaksFlowrate,
    decimal? MaksKapasitasMeterM3Jam,
    short? DurasiPelaksanaanBulan,
    StatusRkap? StatusRkap,
    SkemaPembayaran? SkemaPembayaran,
    string? JaminanStatus,
    string? JaminanJenis,
    string? JaminanMasaBerlaku,
    string? JaminanPenerbit,
    decimal? KetersediaanPasokanBbtud,
    string? AnalisisKomersial,
    string? AnalisisKompetitor,
    string? Kesimpulan,
    decimal? RadiusKompetitorKm,
    IReadOnlyList<NolEvaluationScenarioDetail> Scenarios
);

public sealed record NolIssuanceApprovedTermDetail(
    Guid Id,
    DateOnly PeriodeMulai,
    DateOnly PeriodeSelesai,
    decimal RataRata,
    decimal KontrakMinimum,
    decimal KontrakMaksimum,
    short SortOrder
);

public sealed record NolIssuanceDetail(
    Guid NolRequestId,
    NolOutcome Outcome,
    string? NomorNotaDinas,
    IReadOnlyList<string> KontrakBersyarat,
    DateOnly? BerlakuSejak,
    DateOnly? BerlakuSampai,
    Guid? SignedByUserId,
    DateTimeOffset? SignedAt,
    Guid? DocumentId,
    IReadOnlyList<NolIssuanceApprovedTermDetail> ApprovedTerms
);

public sealed record SaveNolIssuanceRequest(
    NolOutcome Outcome,
    string? NomorNotaDinas,
    IReadOnlyList<string> KontrakBersyarat,
    DateOnly? BerlakuSejak,
    DateOnly? BerlakuSampai,
    Guid? DocumentId,
    IReadOnlyList<NolIssuanceApprovedTermDetail> ApprovedTerms
);
