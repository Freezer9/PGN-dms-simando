using DocumentFormat.OpenXml.Packaging;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Nol;
using Simando.Domain.Organisation;
using Simando.Domain.Registration;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Documents;
using Simando.Infrastructure.Persistence;
using Simando.Integration.Tests;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Documents;

public class DocxDocumentGeneratorTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = NewContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    [Fact(DisplayName = "GenerateKk0DocxAsync produces valid OpenXml .docx bytes containing KK0 sections")]
    public async Task GenerateKk0DocxAsync_ValidCompany_ProducesValidDocxStream()
    {
        var companyId = await SeedDataAsync();
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        var bytes = await generator.GenerateKk0DocxAsync(companyId);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        doc.MainDocumentPart.ShouldNotBeNull();
        var text = doc.MainDocumentPart!.Document!.Body!.InnerText;

        text.ShouldContain("FORMULIR KK0 - DATA SURVEY PASAR");
        text.ShouldContain("PT Industri Prima");
        text.ShouldContain("Boiler Utamabesar");
        text.ShouldContain("150.00 MMBtu/Bulan");
    }

    [Fact(DisplayName = "GenerateA1DocxAsync produces valid OpenXml .docx bytes containing A1 sections")]
    public async Task GenerateA1DocxAsync_ValidCompany_ProducesValidDocxStream()
    {
        var companyId = await SeedDataAsync();
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        var bytes = await generator.GenerateA1DocxAsync(companyId);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        doc.MainDocumentPart.ShouldNotBeNull();
        var text = doc.MainDocumentPart!.Document!.Body!.InnerText;

        text.ShouldContain("FORMULIR REGISTRASI BERLANGGANAN GAS (A1)");
        text.ShouldContain("PT Industri Prima");
        text.ShouldContain("Bpk. Hidayat");
        text.ShouldContain("USD 9.50 / MMBtu");
    }

    [Fact(DisplayName = "GenerateNolRequestDocxAsync produces valid OpenXml .docx bytes containing NOL request sections")]
    public async Task GenerateNolRequestDocxAsync_ValidCompany_ProducesValidDocxStream()
    {
        var companyId = await SeedDataAsync();
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        var bytes = await generator.GenerateNolRequestDocxAsync(companyId);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        doc.MainDocumentPart.ShouldNotBeNull();
        var text = doc.MainDocumentPart!.Document!.Body!.InnerText;

        text.ShouldContain("NOTA DINAS PERMOHONAN NOL / RL");
        text.ShouldContain("Lampiran 15");
        text.ShouldContain("PT Industri Prima");
        text.ShouldContain("Biaya Penyambungan Reguler");
        text.ShouldContain("Belum termasuk PPN");
    }

    [Fact(DisplayName = "GenerateEvaluationResumeDocxAsync produces valid OpenXml .docx bytes containing evaluation resume sections")]
    public async Task GenerateEvaluationResumeDocxAsync_ValidCompany_ProducesValidDocxStream()
    {
        var companyId = await SeedDataAsync();
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        var bytes = await generator.GenerateEvaluationResumeDocxAsync(companyId);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        doc.MainDocumentPart.ShouldNotBeNull();
        var text = doc.MainDocumentPart!.Document!.Body!.InnerText;

        text.ShouldContain("RESUME EVALUASI KELAYAKAN COMMERCIAL & TECHNICAL");
        text.ShouldContain("Lampiran 17");
        text.ShouldContain("PT Industri Prima");
        text.ShouldContain("Skenario Utama");
        text.ShouldContain("14.50 %");
    }

    [Fact(DisplayName = "GenerateNolIssuanceDocxAsync produces valid OpenXml .docx bytes containing NOL issuance sections")]
    public async Task GenerateNolIssuanceDocxAsync_ValidCompany_ProducesValidDocxStream()
    {
        var companyId = await SeedDataAsync();
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        var bytes = await generator.GenerateNolIssuanceDocxAsync(companyId);

        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);

        using var ms = new MemoryStream(bytes);
        using var doc = WordprocessingDocument.Open(ms, false);

        doc.MainDocumentPart.ShouldNotBeNull();
        var text = doc.MainDocumentPart!.Document!.Body!.InnerText;

        text.ShouldContain("SURAT PENERBITAN NOTICE OF LETTER (NOL)");
        text.ShouldContain("PT Industri Prima");
        text.ShouldContain("0101.PK/PR.01.01/BKS/2026");
        text.ShouldContain("Pemberitahuan harian 24 jam sebelum pemakaian.");
    }

    [Fact(DisplayName = "GenerateKk0DocxAsync throws InvalidOperationException when company does not exist")]
    public async Task GenerateKk0DocxAsync_UnknownCompany_Throws()
    {
        var generator = new DocxDocumentGenerator(new SingleContextFactory(_container.GetConnectionString()));

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await generator.GenerateKk0DocxAsync(Guid.NewGuid()));
    }

    private async Task<Guid> SeedDataAsync()
    {
        await using var db = NewContext();

        var provinceId = Guid.NewGuid();
        var regencyId = Guid.NewGuid();
        var districtId = Guid.NewGuid();
        var villageId = Guid.NewGuid();
        var industryTypeId = Guid.NewGuid();
        var regionId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        db.Provinces.Add(new Province { Id = provinceId, BpsCode = "32", Name = "Jawa Barat" });
        db.Regencies.Add(new Regency { Id = regencyId, ProvinceId = provinceId, BpsCode = "3275", Type = RegencyType.Kota, Name = "Bekasi" });
        db.Districts.Add(new District { Id = districtId, RegencyId = regencyId, BpsCode = "327501", Name = "Bekasi Selatan" });
        db.Villages.Add(new Village { Id = villageId, DistrictId = districtId, BpsCode = "3275011001", Type = VillageType.Kelurahan, Name = "Kayuringin Jaya" });
        db.IndustryTypes.Add(new IndustryType { Id = industryTypeId, Name = "Manufaktur Tekstil" });
        db.Regions.Add(new Region { Id = regionId, Code = "SOR1", Name = "SOR 1 Sumatra West Java", Active = true });
        db.Areas.Add(new Area { Id = areaId, RegionId = regionId, Code = "BKS", Name = "Bekasi", Active = true });

        var creator = new Simando.Infrastructure.Identity.ApplicationUser { Id = Guid.NewGuid(), UserName = "creator", FullName = "Creator" };
        db.Users.Add(creator);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            NomorSeq = 101,
            Nomor = "0101-32-3275",
            NamaPerusahaan = "PT Industri Prima",
            VillageId = villageId,
            Alamat = "Jl. Raya Industri No. 45, Bekasi",
            IndustryTypeId = industryTypeId,
            AreaId = areaId,
            CurrentStage = 6,
            Status = RecordStatus.Draft,
            CreatedBy = creator.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            Npwp = "01.234.567.8-012.000",
            Email = "info@industriprima.co.id",
            Telp = "021-88991234"
        };
        db.Companies.Add(company);

        db.Surveys.Add(new Survey
        {
            CompanyId = companyId,
            TanggalSurvey = DateOnly.FromDateTime(DateTime.UtcNow),
            JumlahKaryawan = 150,
            JumlahShift = 3,
            JamKerjaPerHari = 24,
            HariPerMinggu = 7,
            PipaTerdekatJarakM = 250,
            JumlahKebutuhanEnergi = 150.00m
        });

        db.SurveyEquipment.Add(new SurveyEquipment
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            JenisPeralatan = "Boiler Utamabesar",
            Kapasitas = 10,
            JamPerHari = 24,
            HariPerMinggu = 7,
            KonversiKeGas = 150.00m,
            SortOrder = 1
        });

        db.A1Registrations.Add(new A1Registration
        {
            CompanyId = companyId,
            TanggalRegistrasi = DateOnly.FromDateTime(DateTime.UtcNow),
            NamaPenanggungJawab = "Bpk. Hidayat",
            Jabatan = "Direktur Utama",
            BasisKontrak = BasisKontrak.Bulanan,
            SkemaHarga = SkemaHarga.Reguler,
            HargaNilai = 9.50m,
            HargaCurrency = HargaCurrency.USD,
            HargaUnit = HargaUnit.MMBtu
        });

        db.A1UsagePeriods.Add(new A1UsagePeriod
        {
            A1RegistrationCompanyId = companyId,
            PeriodeMulai = new DateOnly(2026, 9, 1),
            PeriodeSelesai = new DateOnly(2027, 8, 31),
            RataRata = 150m,
            Minimum = 120m,
            Maksimum = 180m,
            SortOrder = 1
        });

        db.NolRequests.Add(new NolRequest
        {
            CompanyId = companyId,
            BulanDimulai = new DateOnly(2026, 9, 1),
            SamaDenganA1 = true,
            BasisKontrak = BasisKontrak.Bulanan,
            SkemaHarga = SkemaHarga.Reguler,
            HargaNilai = 9.50m,
            HargaCurrency = HargaCurrency.USD,
            HargaUnit = HargaUnit.MMBtu,
            BiayaPenyambunganReguler = 25000000m,
            BiayaPenyambunganExtra = 0m
        });

        db.NolEvaluations.Add(new NolEvaluation
        {
            NolRequestId = companyId,
            FeedStatus = FeedStatus.Selesai,
            FeedCompletedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            CapexFinal = 500000000m,
            PipaIndukPanjangM = 150m,
            PipaIndukDiameter = 4m,
            PipaIndukDiameterUnit = DiameterUnit.Inch,
            StatusRkap = StatusRkap.Rkap,
            SkemaPembayaran = SkemaPembayaran.PembayaranDimuka,
            KetersediaanPasokanBbtud = 0.15m,
            Kesimpulan = "Permohonan NOL Berlangganan Gas Layak untuk Disetujui.",
            Scenarios =
            [
                new NolEvaluationScenario
                {
                    NolEvaluationNolRequestId = companyId,
                    Label = "Skenario Utama",
                    IrrPct = 14.50m,
                    Npv = 150000000m,
                    PaybackYears = 3.2m,
                    HasilAnalisis = "Layak Secara Komersial"
                }
            ]
        });

        db.NolIssuances.Add(new NolIssuance
        {
            NolRequestId = companyId,
            Outcome = NolOutcome.Nol,
            NomorNotaDinas = "0101.PK/PR.01.01/BKS/2026",
            BerlakuSejak = new DateOnly(2026, 9, 1),
            BerlakuSampai = new DateOnly(2027, 8, 31),
            KontrakBersyarat = ["Pemberitahuan harian 24 jam sebelum pemakaian."],
            ApprovedTerms =
            [
                new NolIssuanceApprovedTerm
                {
                    NolIssuanceNolRequestId = companyId,
                    PeriodeMulai = new DateOnly(2026, 9, 1),
                    PeriodeSelesai = new DateOnly(2027, 8, 31),
                    RataRata = 0.15m,
                    KontrakMinimum = 0.12m,
                    KontrakMaksimum = 0.18m,
                    SortOrder = 1
                }
            ]
        });

        await db.SaveChangesAsync();
        return companyId;
    }

    private SimandoDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<SimandoDbContext>()
            .UseNpgsql(_container.GetConnectionString(), npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SimandoDbContext(options, new UnrestrictedCurrentUser());
    }

    private sealed class SingleContextFactory(string connectionString) : IDbContextFactory<SimandoDbContext>
    {
        public SimandoDbContext CreateDbContext() => Build();
        public Task<SimandoDbContext> CreateDbContextAsync(CancellationToken ct = default) => Task.FromResult(Build());

        private SimandoDbContext Build()
        {
            var options = new DbContextOptionsBuilder<SimandoDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention()
                .Options;

            return new SimandoDbContext(options, new UnrestrictedCurrentUser());
        }
    }
}
