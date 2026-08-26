using Simando.Domain.Registration;

namespace Simando.Application.Registration;

public sealed record A1UsagePeriodDetail(
    Guid Id,
    DateOnly PeriodeMulai,
    DateOnly PeriodeSelesai,
    decimal RataRata,
    decimal Minimum,
    decimal Maksimum,
    short SortOrder
);

public sealed record A1RegistrationDetail(
    Guid CompanyId,
    DateOnly? TanggalRegistrasi,
    RegistrasiSource RegistrasiSource,
    string? NamaPenanggungJawab,
    string? Jabatan,
    DateOnly? BulanDimulai,
    BasisKontrak? BasisKontrak,
    SkemaHarga? SkemaHarga,
    Guid? SegmentId,
    string? KodeHarga,
    decimal? HargaNilai,
    HargaCurrency? HargaCurrency,
    HargaUnit? HargaUnit,
    decimal? CapexAwal,
    bool MomSigasTersedia,
    StatusBangunan? StatusBangunan,
    Sektor? Sektor,
    string? ProduksiUtama,
    string? JenisPeralatanGas,
    decimal? TekananOperasiBarg,
    Guid? SignedDocumentId,
    SignatureMethod? SignatureMethod,
    IReadOnlyList<A1UsagePeriodDetail> UsagePeriods
);

public sealed record SaveA1RegistrationRequest(
    DateOnly? TanggalRegistrasi,
    string? NamaPenanggungJawab,
    string? Jabatan,
    DateOnly? BulanDimulai,
    BasisKontrak? BasisKontrak,
    SkemaHarga? SkemaHarga,
    Guid? SegmentId,
    string? KodeHarga,
    decimal? HargaNilai,
    HargaCurrency? HargaCurrency,
    HargaUnit? HargaUnit,
    decimal? CapexAwal,
    bool MomSigasTersedia,
    StatusBangunan? StatusBangunan,
    Sektor? Sektor,
    string? ProduksiUtama,
    string? JenisPeralatanGas,
    decimal? TekananOperasiBarg,
    Guid? SignedDocumentId,
    SignatureMethod? SignatureMethod,
    IReadOnlyList<A1UsagePeriodDetail> UsagePeriods
);
