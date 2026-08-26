using Simando.Domain.Registration;

namespace Simando.Domain.Nol;

// Stage 6 (NOL Request) header — 1:1 extension of Company, CompanyId is the primary key.
// docs/design/data-model.md#nol_request--stage-6
public sealed class NolRequest
{
    public required Guid CompanyId { get; init; }

    public string? NomorNotaDinas { get; set; }
    public RegistrationType RegistrationType { get; set; } = RegistrationType.RegistrasiBaru;
    public bool SamaDenganA1 { get; set; }

    public DateOnly? BulanDimulai { get; set; }
    public BasisKontrak? BasisKontrak { get; set; }
    public SkemaHarga? SkemaHarga { get; set; }
    public Guid? SegmentId { get; set; }
    public string? KodeHarga { get; set; }
    public decimal? HargaNilai { get; set; }
    public HargaCurrency? HargaCurrency { get; set; }
    public HargaUnit? HargaUnit { get; set; }

    public string? AlasanKontrakBersyarat { get; set; }
    public string? NamaPimpinanPerusahaan { get; set; }
    public string? JangkaWaktuKontrak { get; set; }

    public decimal? CapexPreGr3 { get; set; }
    public decimal? BiayaPenyambunganReguler { get; set; }
    public decimal? BiayaPenyambunganExtra { get; set; }

    // Biaya penyambungan jumlah is derived from reguler + extra
    public decimal BiayaPenyambunganJumlah => (BiayaPenyambunganReguler ?? 0) + (BiayaPenyambunganExtra ?? 0);

    public Guid? WorkflowInstanceId { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }

    public List<NolRequestPeriod> Periods { get; set; } = [];
    public List<NolRequestDaily> DailyBasisRows { get; set; } = [];
    public List<NolRequestReference> References { get; set; } = [];
}
