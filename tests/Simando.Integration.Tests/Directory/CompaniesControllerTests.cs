using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Simando.Api.Controllers;
using Simando.Application.Common;
using Simando.Application.Directory;
using Simando.Application.Nol;
using Simando.Application.Registration;
using Simando.Application.Security;
using Simando.Domain.Directory;
using Simando.Domain.Geography;
using Simando.Domain.MasterData;
using Simando.Domain.Organisation;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.Integration.Tests.Directory;

public class CompaniesControllerTests : IAsyncLifetime
{
    private const string SalesEmail = "sales@pgn.co.id";
    private const string SalesInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string SalesPassword = "New-Correct-Horse-Password-1";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("imresamu/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private Guid _regionId;
    private Guid _areaId;
    private Guid _villageId;
    private Guid _industryTypeId;
    private Guid _segmentId;
    private Guid _fuelTypeId;
    private Guid _salesUserId;
    private string _salesTemporaryPassword = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder
                .ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Postgres"] = _container.GetConnectionString(),
                        ["Storage:Type"] = "S3",
                        ["Storage:S3:ServiceUrl"] = "http://localhost:9000",
                        ["Storage:S3:Bucket"] = "simando",
                        ["Storage:S3:AccessKey"] = "test",
                        ["Storage:S3:SecretKey"] = "test",
                    }))
                .ConfigureServices(services => services.RemoveAll<IHostedService>()));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            await db.Database.MigrateAsync();

            var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
            await seeder.SeedAsync("admin", "Admin-Pass-12345!", "System Admin", email: "admin@pgn.co.id");

            _regionId = Guid.NewGuid();
            _areaId = Guid.NewGuid();
            db.Regions.Add(new Region { Id = _regionId, Code = "SOR3", Name = "Region 3", Active = true });
            db.Areas.Add(new Area { Id = _areaId, RegionId = _regionId, Code = "SBY", Name = "Area Surabaya", Active = true });

            var province = new Province { Id = Guid.NewGuid(), BpsCode = "35", Name = "Jawa Timur" };
            var regency = new Regency { Id = Guid.NewGuid(), ProvinceId = province.Id, BpsCode = "3578", Type = RegencyType.Kota, Name = "Kota Surabaya" };
            var district = new District { Id = Guid.NewGuid(), RegencyId = regency.Id, BpsCode = "357801", Name = "Tegalsari" };
            var village = new Village { Id = Guid.NewGuid(), DistrictId = district.Id, BpsCode = "3578011001", Type = VillageType.Kelurahan, Name = "Tegalsari" };
            _villageId = village.Id;

            db.Provinces.Add(province);
            db.Regencies.Add(regency);
            db.Districts.Add(district);
            db.Villages.Add(village);

            var industryType = new IndustryType { Id = Guid.NewGuid(), Name = "Manufaktur Makanan" };
            _industryTypeId = industryType.Id;
            db.IndustryTypes.Add(industryType);

            var segment = new Segment { Id = Guid.NewGuid(), Name = "Gold", SortOrder = 1 };
            _segmentId = segment.Id;
            db.Segments.Add(segment);

            var fuelType = new FuelType { Id = Guid.NewGuid(), Name = "Solar / HSD" };
            _fuelTypeId = fuelType.Id;
            db.FuelTypes.Add(fuelType);

            await db.SaveChangesAsync();

            // Create sales user with SalesArea role scoped to Area Surabaya
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var adminActor = new EffectivePermissions(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());
            var createResult = await userService.CreateUserAsync(
                "Sales Surabaya", "sales.sby", SalesEmail,
                Role.SalesArea, _areaId, _regionId,
                Guid.Empty, adminActor);

            _salesUserId = createResult.UserId;
            _salesTemporaryPassword = createResult.TemporaryPassword!;
        }

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Sign in as sales user
        var loginRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(SalesEmail, _salesTemporaryPassword));
        loginRes.EnsureSuccessStatusCode();
        var changeRes = await _client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(_salesTemporaryPassword, SalesPassword));
        changeRes.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact(DisplayName = "Full HTTP lifecycle: Create -> Plotting -> Contact -> Survey -> A1 -> GetDetail")]
    public async Task Company_HttpLifecycle_Succeeds()
    {
        // 1. Create company prospect
        var createRequest = new CreateCompanyRequest(
            "PT Industri Maju Bersama",
            "https://indumaju.co.id",
            _villageId,
            "Jl. Basuki Rahmat No. 12",
            -7.2575,
            112.7521,
            _industryTypeId,
            _areaId,
            "info@indumaju.co.id",
            "60261",
            "031-5551234",
            "01.234.567.8-012.000");

        var createResponse = await _client.PostAsJsonAsync("/api/companies", createRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateCompanyResult>(JsonOptions);
        createResult.ShouldNotBeNull();
        var companyId = createResult.CompanyId;
        companyId.ShouldNotBe(Guid.Empty);

        // 2. Save plotting data
        var plottingRequest = new SavePlottingRequest(_salesUserId, PosisiPelanggan.JalurExisting, Kawasan.KawasanIndustri);
        var plottingResponse = await _client.PutAsJsonAsync($"/api/companies/{companyId}/plotting", plottingRequest);
        plottingResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 3. Add contact PIC
        var contactRequest = new SaveContactRequest(
            "Bambang Hermanto",
            "Direktur Operasional",
            "bambang@indumaju.co.id",
            "081234567890",
            "https://linkedin.com/in/bambang-hermanto",
            null,
            null,
            true);

        var contactResponse = await _client.PostAsJsonAsync($"/api/companies/{companyId}/contacts", contactRequest);
        contactResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 4. Save Survey KK0
        var surveyPayload = new SaveSurveyFullPayload(
            new SaveSurveyRequest(
                TanggalSurvey: new DateOnly(2025, 6, 1),
                SurveyorUserId: _salesUserId,
                JumlahKaryawan: 120,
                JumlahShift: 2,
                JamKerjaPerHari: 16,
                HariPerMinggu: 6,
                BebanPuncak1Mulai: null,
                BebanPuncak1Selesai: null,
                BebanPuncak2Mulai: null,
                BebanPuncak2Selesai: null,
                KebutuhanEnergi: KebutuhanEnergiJenis.Listrik,
                KebutuhanEnergiLainnya: null,
                KapasitasNilai: 1000,
                KapasitasUnitId: null,
                PemakaianNilai: 800,
                PemakaianUnitId: null,
                PipaTerdekatJarakM: 250,
                PipaTerdekatDiameter: 4,
                PipaTerdekatTekanan: 4,
                BahanBakarEksisting: BahanBakarEksisting.Hsd,
                NamaPemasok: "Pertamina",
                KapasitasListrikKw: 500,
                PemakaianListrikKwh: 35000,
                RencanaPemanfaatanGas: RencanaPemanfaatanGas.BahanBakar,
                DeskripsiProsesProduksi: "Proses pembakaran boiler",
                MinEfisiensiDiharapkanPct: 85,
                WillingnessToPayUsdMmbtu: 9.5m,
                KeteranganLain: "Prioritas konversi gas"),
            [new SaveSurveyProductRequest("Biskuit Gandum", 5000, 25000, "Kemasan 500g")],
            [new SaveSurveyRawMaterialRequest("Tepung Terigu", Asal.Lokal, null, 4500, null)],
            [new SaveSurveyMarketRequest("Pasar Domestik", Asal.Lokal, null, 80, null)],
            [new SaveSurveyEquipmentRequest("Boiler Steam 5T", 5, null, 16, 6, _fuelTypeId, 12000, 5000, null, 1500)]);

        var surveyResponse = await _client.PutAsJsonAsync($"/api/companies/{companyId}/survey", surveyPayload);
        surveyResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Seed KK0 attachment for Stage Gate 4 -> 5
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            db.Attachments.Add(new Simando.Domain.Attachments.Attachment
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                AttachableType = "Company",
                AttachableId = companyId,
                Kind = Simando.Domain.Attachments.AttachmentKind.Kk0,
                Filename = "kk0.pdf",
                MimeType = "application/pdf",
                SizeBytes = 1024,
                Checksum = "sha256-dummy-kk0",
                StorageKey = "attachments/kk0.pdf",
                StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                UploadedBy = _salesUserId,
                UploadedAt = DateTimeOffset.UtcNow,
                Version = 1
            });
            await db.SaveChangesAsync();
        }

        // 5. Save A1 Registration
        var a1Request = new SaveA1RegistrationRequest(
            TanggalRegistrasi: new DateOnly(2025, 7, 1),
            NamaPenanggungJawab: "Bambang Hermanto",
            Jabatan: "Direktur Operasional",
            BulanDimulai: new DateOnly(2026, 1, 1),
            BasisKontrak: BasisKontrak.Bulanan,
            SkemaHarga: SkemaHarga.Reguler,
            SegmentId: _segmentId,
            KodeHarga: "REG-01",
            HargaNilai: 10.5m,
            HargaCurrency: HargaCurrency.USD,
            HargaUnit: HargaUnit.MMBtu,
            CapexAwal: 500000000m,
            MomSigasTersedia: true,
            StatusBangunan: StatusBangunan.Eksisting,
            Sektor: Sektor.Industri,
            ProduksiUtama: "Biskuit Gandum",
            JenisPeralatanGas: "Boiler Steam",
            TekananOperasiBarg: 4.0m,
            SignedDocumentId: null,
            SignatureMethod: SignatureMethod.Digital,
            UsagePeriods: []);

        var a1Response = await _client.PutAsJsonAsync($"/api/companies/{companyId}/registration", a1Request);
        a1Response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // 6. Get Detail and verify
        var detailResponse = await _client.GetAsync($"/api/companies/{companyId}");
        detailResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var detail = await detailResponse.Content.ReadFromJsonAsync<CompanyRecordDto>(JsonOptions);
        detail.ShouldNotBeNull();
        detail.Id.ShouldBe(companyId);
        detail.NamaPerusahaan.ShouldBe("PT Industri Maju Bersama");
        detail.CurrentStage.ShouldBe((byte)5);
        detail.Contacts.Count.ShouldBeGreaterThan(0);
        detail.Contacts[0].Nama.ShouldBe("Bambang Hermanto");
    }

    [Fact(DisplayName = "GET /api/companies returns paged list of companies")]
    public async Task GetCompaniesList_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/companies?page=1&pageSize=10");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<CompanyListItem>>(JsonOptions);
        result.ShouldNotBeNull();
        result.Page.ShouldBe(1);
    }
}
