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
using Simando.Domain.Nol;
using Simando.Domain.Organisation;
using Simando.Domain.Registration;
using Simando.Domain.Security;
using Simando.Domain.Survey;
using Simando.Domain.Workflow;
using Simando.Infrastructure.Identity;
using Simando.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Simando.E2E.Tests;

public class FullSalesPipelineE2ETests : IAsyncLifetime
{
    private const string SalesEmail = "sales.pipeline@pgn.co.id";
    private const string AreaHeadEmail = "head.pipeline@pgn.co.id";
    private const string RegAdminEmail = "regadmin.pipeline@pgn.co.id";
    private const string Reviewer1Email = "reviewer1.pipeline@pgn.co.id";
    private const string Reviewer2Email = "reviewer2.pipeline@pgn.co.id";
    private const string DivHeadEmail = "divhead.pipeline@pgn.co.id";
    private const string DefaultInitialPassword = "Correct-Horse-Battery-Staple-1";
    private const string DefaultPassword = "New-Correct-Horse-Password-1";

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:18-3.6-alpine")
        .WithDatabase("simando")
        .WithUsername("simando")
        .WithPassword("simando")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private Guid _regionId;
    private Guid _areaId;
    private Guid _villageId;
    private Guid _industryTypeId;
    private Guid _segmentId;
    private Guid _fuelTypeId;
    private Guid _salesUserId;
    private Guid _areaHeadUserId;
    private Guid _regAdminUserId;
    private Guid _reviewer1UserId;
    private Guid _reviewer2UserId;
    private Guid _divHeadUserId;
    private readonly Dictionary<string, string> _userTemporaryPasswords = new();

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
            await seeder.SeedAsync("admin.pipeline", "Admin-Pass-12345!", "Admin Pipeline", email: "admin.pipeline@pgn.co.id");

            _regionId = Guid.NewGuid();
            _areaId = Guid.NewGuid();
            db.Regions.Add(new Region { Id = _regionId, Code = "SOR3", Name = "Region 3 - Jatim", Active = true });
            db.Areas.Add(new Area { Id = _areaId, RegionId = _regionId, Code = "SDA", Name = "Area Sidoarjo", Active = true });

            var province = new Province { Id = Guid.NewGuid(), BpsCode = "35", Name = "Jawa Timur" };
            var regency = new Regency { Id = Guid.NewGuid(), ProvinceId = province.Id, BpsCode = "3515", Type = RegencyType.Kabupaten, Name = "Kabupaten Sidoarjo" };
            var district = new District { Id = Guid.NewGuid(), RegencyId = regency.Id, BpsCode = "351508", Name = "Waru" };
            var village = new Village { Id = Guid.NewGuid(), DistrictId = district.Id, BpsCode = "3515082001", Type = VillageType.Desa, Name = "Tropodo" };
            _villageId = village.Id;

            db.Provinces.Add(province);
            db.Regencies.Add(regency);
            db.Districts.Add(district);
            db.Villages.Add(village);

            var industryType = new IndustryType { Id = Guid.NewGuid(), Name = "Industri Keramik & Kaca" };
            _industryTypeId = industryType.Id;
            db.IndustryTypes.Add(industryType);

            var segment = new Segment { Id = Guid.NewGuid(), Name = "Gold", SortOrder = 1 };
            _segmentId = segment.Id;
            db.Segments.Add(segment);

            var fuelType = new FuelType { Id = Guid.NewGuid(), Name = "LPG Industri" };
            _fuelTypeId = fuelType.Id;
            db.FuelTypes.Add(fuelType);

            await db.SaveChangesAsync();

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var adminActor = new EffectivePermissions(AccessScope.All, null, null, Enum.GetValues<Capability>().ToHashSet());

            async Task<Guid> CreateUserWithRole(string name, string username, string email, Role role, Guid? areaId, Guid? regionId)
            {
                var res = await userService.CreateUserAsync(name, username, email, role, areaId, regionId, Guid.Empty, adminActor);
                _userTemporaryPasswords[email] = res.TemporaryPassword!;
                return res.UserId;
            }

            _salesUserId = await CreateUserWithRole("Sales Sidoarjo", "sales.sda", SalesEmail, Role.SalesArea, _areaId, _regionId);
            _areaHeadUserId = await CreateUserWithRole("Kepala Area Sidoarjo", "head.sda", AreaHeadEmail, Role.AreaHead, _areaId, _regionId);
            _regAdminUserId = await CreateUserWithRole("Admin Regional Jatim", "regadmin.sda", RegAdminEmail, Role.RegionalAdmin, null, _regionId);
            _reviewer1UserId = await CreateUserWithRole("Reviewer 1", "reviewer1.sda", Reviewer1Email, Role.Reviewer, null, _regionId);
            _reviewer2UserId = await CreateUserWithRole("Reviewer 2", "reviewer2.sda", Reviewer2Email, Role.Reviewer, null, _regionId);
            _divHeadUserId = await CreateUserWithRole("Kadiv Gas", "divhead.sda", DivHeadEmail, Role.DivisionHead, null, _regionId);
        }
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(string email)
    {
        var tempPassword = _userTemporaryPasswords[email];
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginRes = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, tempPassword));
        loginRes.EnsureSuccessStatusCode();
        var changeRes = await client.PostAsJsonAsync("/api/auth/change-password", new ChangePasswordRequest(tempPassword, DefaultPassword));
        changeRes.EnsureSuccessStatusCode();
        return client;
    }

    [Fact(DisplayName = "Full E2E Pipeline: Prospect -> Plotting -> Contact -> Survey -> A1 -> Submit -> Review -> Approve -> Eval -> NOL Issuance -> Docs")]
    public async Task CompleteSalesPipeline_EndToEnd_Succeeds()
    {
        using var salesClient = await CreateAuthenticatedClientAsync(SalesEmail);

        // Stage 1: Create prospect
        var createRequest = new CreateCompanyRequest(
            "PT Keramik Mulia Jaya",
            "https://keramikmulia.co.id",
            _villageId,
            "Kawasan Industri Rungkut Industri Barat",
            -7.3255,
            112.7688,
            _industryTypeId,
            _areaId,
            "kontak@keramikmulia.co.id",
            "61256",
            "031-8987654",
            "02.345.678.9-015.000");

        var createRes = await salesClient.PostAsJsonAsync("/api/companies", createRequest);
        createRes.StatusCode.ShouldBe(HttpStatusCode.Created);
        var createResult = await createRes.Content.ReadFromJsonAsync<CreateCompanyResult>(JsonOptions);
        createResult.ShouldNotBeNull();
        var companyId = createResult.CompanyId;

        // Stage 2: Save plotting
        var plottingReq = new SavePlottingRequest(_salesUserId, PosisiPelanggan.JalurExisting, Kawasan.KawasanIndustri);
        var plotRes = await salesClient.PutAsJsonAsync($"/api/companies/{companyId}/plotting", plottingReq);
        plotRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Stage 3: Register contact person
        var contactReq = new SaveContactRequest("Hendra Gunawan", "Direktur Pabrik", "hendra@keramikmulia.co.id", "081122334455", null, null, null, true);
        var contactRes = await salesClient.PostAsJsonAsync($"/api/companies/{companyId}/contacts", contactReq);
        contactRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Stage 4: Submit Survey KK0
        var surveyPayload = new SaveSurveyFullPayload(
            new SaveSurveyRequest(
                TanggalSurvey: new DateOnly(2025, 8, 1),
                SurveyorUserId: _salesUserId,
                JumlahKaryawan: 250,
                JumlahShift: 3,
                JamKerjaPerHari: 24,
                HariPerMinggu: 7,
                BebanPuncak1Mulai: null,
                BebanPuncak1Selesai: null,
                BebanPuncak2Mulai: null,
                BebanPuncak2Selesai: null,
                KebutuhanEnergi: KebutuhanEnergiJenis.Listrik,
                KebutuhanEnergiLainnya: null,
                KapasitasNilai: 2500,
                KapasitasUnitId: null,
                PemakaianNilai: 2000,
                PemakaianUnitId: null,
                PipaTerdekatJarakM: 100,
                PipaTerdekatDiameter: 6,
                PipaTerdekatTekanan: 4,
                BahanBakarEksisting: BahanBakarEksisting.Lpg,
                NamaPemasok: "Pertamina Gas Niaga",
                KapasitasListrikKw: 1000,
                PemakaianListrikKwh: 80000,
                RencanaPemanfaatanGas: RencanaPemanfaatanGas.BahanBakar,
                DeskripsiProsesProduksi: "Pembakaran pada roller kiln keramik",
                MinEfisiensiDiharapkanPct: 90,
                WillingnessToPayUsdMmbtu: 10.0m,
                KeteranganLain: "Konversi kiln utama ke gas pipa"),
            [new SaveSurveyProductRequest("Keramik Granit 60x60", 300000, 75000, "Kemasan Box")],
            [new SaveSurveyRawMaterialRequest("Tanah Liat & Feldspar", Asal.Lokal, null, 8000, null)],
            [new SaveSurveyMarketRequest("Domestik & Ekspor ASEAN", Asal.Lokal, null, 100, null)],
            [new SaveSurveyEquipmentRequest("Kiln Tunnel Roller 1", 20, null, 24, 7, _fuelTypeId, 15000, 10000, null, 3500)]);

        var surveyRes = await salesClient.PutAsJsonAsync($"/api/companies/{companyId}/survey", surveyPayload);
        surveyRes.StatusCode.ShouldBe(HttpStatusCode.OK);

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

        // Stage 5: Submit A1 Registration
        var a1Req = new SaveA1RegistrationRequest(
            TanggalRegistrasi: new DateOnly(2025, 9, 1),
            NamaPenanggungJawab: "Hendra Gunawan",
            Jabatan: "Direktur Pabrik",
            BulanDimulai: new DateOnly(2026, 4, 1),
            BasisKontrak: BasisKontrak.Bulanan,
            SkemaHarga: SkemaHarga.Reguler,
            SegmentId: _segmentId,
            KodeHarga: "REG-01",
            HargaNilai: 11.2m,
            HargaCurrency: HargaCurrency.USD,
            HargaUnit: HargaUnit.MMBtu,
            CapexAwal: 1200000000m,
            MomSigasTersedia: true,
            StatusBangunan: StatusBangunan.Eksisting,
            Sektor: Sektor.Industri,
            ProduksiUtama: "Keramik Granit",
            JenisPeralatanGas: "Kiln Roller",
            TekananOperasiBarg: 4.0m,
            SignedDocumentId: null,
            SignatureMethod: SignatureMethod.Digital,
            UsagePeriods: []);

        var a1Res = await salesClient.PutAsJsonAsync($"/api/companies/{companyId}/registration", a1Req);
        a1Res.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Seed A1 and BuktiKelayakan attachments for Stage Gate 5 -> 6
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            db.Attachments.AddRange(
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    AttachableType = "Company",
                    AttachableId = companyId,
                    Kind = Simando.Domain.Attachments.AttachmentKind.A1,
                    Filename = "a1.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-a1",
                    StorageKey = "attachments/a1.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                },
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    AttachableType = "Company",
                    AttachableId = companyId,
                    Kind = Simando.Domain.Attachments.AttachmentKind.BuktiKelayakan,
                    Filename = "bukti.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-bukti",
                    StorageKey = "attachments/bukti.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                });
            await db.SaveChangesAsync();
        }

        // Stage 6: Submit NOL Request & CapexPreGr3 Attachment
        var nolReq = new SaveNolRequestRequest(
            NomorNotaDinas: "ND-001/REQ/2025",
            RegistrationType: RegistrationType.RegistrasiBaru,
            SamaDenganA1: true,
            BulanDimulai: new DateOnly(2026, 4, 1),
            BasisKontrak: BasisKontrak.Bulanan,
            SkemaHarga: SkemaHarga.Reguler,
            SegmentId: _segmentId,
            KodeHarga: "REG-01",
            HargaNilai: 11.2m,
            HargaCurrency: HargaCurrency.USD,
            HargaUnit: HargaUnit.MMBtu,
            AlasanKontrakBersyarat: "Penyelesaian konstruksi jaringan pipa",
            NamaPimpinanPerusahaan: "Hendra Gunawan",
            JangkaWaktuKontrak: "5 Tahun",
            CapexPreGr3: 1200000000m,
            BiayaPenyambunganReguler: 50000000m,
            BiayaPenyambunganExtra: 0m,
            Periods: [],
            DailyBasisRows: [],
            ReferenceDocumentIds: []);

        var nolReqRes = await salesClient.PutAsJsonAsync($"/api/companies/{companyId}/nol-request", nolReq);
        nolReqRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            db.Attachments.Add(
                new Simando.Domain.Attachments.Attachment
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    AttachableType = "Company",
                    AttachableId = companyId,
                    Kind = Simando.Domain.Attachments.AttachmentKind.CapexPreGr3,
                    Filename = "capex.pdf",
                    MimeType = "application/pdf",
                    SizeBytes = 1024,
                    Checksum = "sha256-dummy-3",
                    StorageKey = "attachments/capex.pdf",
                    StorageProvider = Simando.Domain.Attachments.StorageProvider.S3,
                    UploadedBy = _salesUserId,
                    UploadedAt = DateTimeOffset.UtcNow,
                    Version = 1
                });
            await db.SaveChangesAsync();
        }

        // Submit for approval (Stage 6)
        var submitRes = await salesClient.PostAsync($"/api/companies/{companyId}/workflow/start", null);
        submitRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 1: AreaHead approves
        Guid activeStepId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var instance = await db.WorkflowInstances.IgnoreQueryFilters().SingleAsync(i => i.CompanyId == companyId);
            var step = await db.WorkflowSteps.IgnoreQueryFilters()
                .SingleAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.AreaHead);
            activeStepId = step.Id;
        }

        using var areaHeadClient = await CreateAuthenticatedClientAsync(AreaHeadEmail);
        var ahActRes = await areaHeadClient.PostAsJsonAsync($"/api/workflow/steps/{activeStepId}/act",
            new ActOnStepRequest(WorkflowAction.Setuju, "Survey dan A1 disetujui Kepala Area."));
        ahActRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 2: RegionalAdmin fills NOL Evaluation, chooses reviewers and approves
        using var regAdminClient = await CreateAuthenticatedClientAsync(RegAdminEmail);

        // Stage 7: Submit NOL Evaluation (during RegionalAdmin step)
        var evalReq = new SaveNolEvaluationRequest(
            FeedStatus: FeedStatus.Selesai,
            FeedCompletedAt: new DateOnly(2026, 2, 15),
            CapexFinal: 1500000000m,
            PipaIndukPanjangM: 500m,
            PipaIndukDiameter: 6m,
            PipaIndukDiameterUnit: DiameterUnit.Inch,
            PipaServicePanjangM: 50m,
            PipaServiceDiameter: 2m,
            PipaServiceDiameterUnit: DiameterUnit.Inch,
            SpesifikasiMrs: "MRS Double Stream 2000",
            GSize: "G40",
            Tekanan: 4.0m,
            MaksFlowrate: 65.0m,
            MaksKapasitasMeterM3Jam: 350.0m,
            DurasiPelaksanaanBulan: 6,
            StatusRkap: StatusRkap.Rkap,
            SkemaPembayaran: SkemaPembayaran.JaminanPembayaran,
            JaminanStatus: "Tervalidasi",
            JaminanJenis: "Cash Collateral",
            JaminanMasaBerlaku: "12 Bulan",
            JaminanPenerbit: "Bank Mandiri",
            KetersediaanPasokanBbtud: 50.0m,
            AnalisisKomersial: "Margin komersial memadai sesuai standar portofolio PGN.",
            AnalisisKompetitor: "Kompetitor terdekat menggunakan CNG dengan harga lebih tinggi.",
            Kesimpulan: "Layak diterbitkan Surat Izin Penyaluran Gas (NOL).",
            RadiusKompetitorKm: 15.0m,
            Scenarios: []);

        var evalRes = await regAdminClient.PutAsJsonAsync($"/api/companies/{companyId}/nol-evaluation", evalReq);
        evalRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        var chooseReviewersRes = await regAdminClient.PostAsJsonAsync($"/api/companies/{companyId}/workflow/choose-reviewers",
            new ChooseReviewersRequest([_reviewer1UserId, _reviewer2UserId]));
        chooseReviewersRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var instance = await db.WorkflowInstances.IgnoreQueryFilters().SingleAsync(i => i.CompanyId == companyId);
            var step = await db.WorkflowSteps.IgnoreQueryFilters()
                .SingleAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.RegionalAdmin);
            activeStepId = step.Id;
        }

        var regActRes = await regAdminClient.PostAsJsonAsync($"/api/workflow/steps/{activeStepId}/act",
            new ActOnStepRequest(WorkflowAction.Setuju, "Reviewer telah ditetapkan dan disetujui Admin Regional."));
        regActRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 3: Reviewer 1 approves
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var instance = await db.WorkflowInstances.IgnoreQueryFilters().SingleAsync(i => i.CompanyId == companyId);
            var step = await db.WorkflowSteps.IgnoreQueryFilters()
                .SingleAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.Reviewer1);
            activeStepId = step.Id;
        }

        using var reviewer1Client = await CreateAuthenticatedClientAsync(Reviewer1Email);
        var rev1ActRes = await reviewer1Client.PostAsJsonAsync($"/api/workflow/steps/{activeStepId}/act",
            new ActOnStepRequest(WorkflowAction.Setuju, "Data teknis telah diperiksa dan disetujui Reviewer 1."));
        rev1ActRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 4: Reviewer 2 approves
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var instance = await db.WorkflowInstances.IgnoreQueryFilters().SingleAsync(i => i.CompanyId == companyId);
            var step = await db.WorkflowSteps.IgnoreQueryFilters()
                .SingleAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.Reviewer2);
            activeStepId = step.Id;
        }

        using var reviewer2Client = await CreateAuthenticatedClientAsync(Reviewer2Email);
        var rev2ActRes = await reviewer2Client.PostAsJsonAsync($"/api/workflow/steps/{activeStepId}/act",
            new ActOnStepRequest(WorkflowAction.Setuju, "Data komersial telah diperiksa dan disetujui Reviewer 2."));
        rev2ActRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Step 5: Division Head fills Stage 8 (NOL Issuance) and approves
        using var divHeadClient = await CreateAuthenticatedClientAsync(DivHeadEmail);

        // Stage 8: Submit NOL Issuance (during DivisionHead step)
        var issuanceReq = new SaveNolIssuanceRequest(
            Outcome: NolOutcome.Nol,
            NomorNotaDinas: "ND-PGN-2026-0099",
            KontrakBersyarat: ["Penyelesaian pipa dinas tepat waktu", "Penyediaan jaminan pembayaran"],
            BerlakuSejak: new DateOnly(2026, 3, 1),
            BerlakuSampai: new DateOnly(2027, 3, 1),
            DocumentId: null,
            ApprovedTerms: []);

        var issuanceRes = await divHeadClient.PutAsJsonAsync($"/api/companies/{companyId}/nol-issuance", issuanceReq);
        issuanceRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SimandoDbContext>();
            var instance = await db.WorkflowInstances.IgnoreQueryFilters().SingleAsync(i => i.CompanyId == companyId);
            var step = await db.WorkflowSteps.IgnoreQueryFilters()
                .SingleAsync(s => s.WorkflowInstanceId == instance.Id && s.Kind == WorkflowStepKind.DivisionHead);
            activeStepId = step.Id;
        }

        var divActRes = await divHeadClient.PostAsJsonAsync($"/api/workflow/steps/{activeStepId}/act",
            new ActOnStepRequest(WorkflowAction.Setuju, "Disetujui penuh oleh Kadiv Gas, diterbitkan NOL."));
        divActRes.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify Final Company Status
        var detailRes = await salesClient.GetAsync($"/api/companies/{companyId}");
        detailRes.StatusCode.ShouldBe(HttpStatusCode.OK);
        var finalDetail = await detailRes.Content.ReadFromJsonAsync<CompanyRecordDto>(JsonOptions);
        finalDetail.ShouldNotBeNull();
        finalDetail.Status.ShouldBe(RecordStatus.IssuedNol);
        finalDetail.CurrentStage.ShouldBe((byte)8);

        // Verify document generation downloads
        var kk0Doc = await salesClient.GetAsync($"/api/documents/company/{companyId}/kk0");
        kk0Doc.StatusCode.ShouldBe(HttpStatusCode.OK);
        kk0Doc.Content.Headers.ContentType?.MediaType.ShouldBe("application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        var a1Doc = await salesClient.GetAsync($"/api/documents/company/{companyId}/a1");
        a1Doc.StatusCode.ShouldBe(HttpStatusCode.OK);

        var evalDoc = await salesClient.GetAsync($"/api/documents/company/{companyId}/evaluation");
        evalDoc.StatusCode.ShouldBe(HttpStatusCode.OK);

        var issuanceDoc = await salesClient.GetAsync($"/api/documents/company/{companyId}/nol-issuance");
        issuanceDoc.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}