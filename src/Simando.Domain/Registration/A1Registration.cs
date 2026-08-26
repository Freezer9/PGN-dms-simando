namespace Simando.Domain.Registration;

// Stage 5 (A1 Registration) header — 1:1 extension of Company, CompanyId is the primary key.
// docs/design/data-model.md#a1_registration--stage-5
public sealed class A1Registration
{
    public required Guid CompanyId { get; init; }

    public DateOnly? TanggalRegistrasi { get; set; }
    public RegistrasiSource RegistrasiSource { get; set; } = RegistrasiSource.Manual;

    public string? NamaPenanggungJawab { get; set; }
    public string? Jabatan { get; set; }

    public DateOnly? BulanDimulai { get; set; }
    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }

    public Guid? SegmentId { get; set; }
    public string? KodeHarga { get; set; }

    public decimal? HargaNilai { get; set; }
    public HargaCurrency? HargaCurrency { get; set; }
    public HargaUnit? HargaUnit { get; set; }

    public decimal? CapexAwal { get; set; }
    public bool MomSigasTersedia { get; set; }

    public StatusBangunan? StatusBangunan { get; set; }
    public Sektor? Sektor { get; set; }
    public string? ProduksiUtama { get; set; }

    public string? JenisPeralatanGas { get; set; }
    public decimal? TekananOperasiBarg { get; set; }

    public Guid? SignedDocumentId { get; set; }
    public SignatureMethod? SignatureMethod { get; set; }

    public List<A1UsagePeriod> UsagePeriods { get; set; } = [];
}
