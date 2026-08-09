using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pgn.Dms.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StageDomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BreakGlassGrants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    GrantedToUserId = table.Column<string>(type: "TEXT", nullable: false),
                    GrantedById = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakGlassGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakGlassGrants_AspNetUsers_GrantedById",
                        column: x => x.GrantedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakGlassGrants_AspNetUsers_GrantedToUserId",
                        column: x => x.GrantedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakGlassGrants_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nama = table.Column<string>(type: "TEXT", nullable: false),
                    Jabatan = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    NoHp = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedIn = table.Column<string>(type: "TEXT", nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", nullable: false),
                    Facebook = table.Column<string>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyContacts_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AttributesJson = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NolIssuances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    NomorNotaDinas = table.Column<string>(type: "TEXT", nullable: false),
                    BerlakuSejak = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BerlakuSampai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SignedById = table.Column<string>(type: "TEXT", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Catatan = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolIssuances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolIssuances_AspNetUsers_SignedById",
                        column: x => x.SignedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NolIssuances_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plottings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SalesUserId = table.Column<string>(type: "TEXT", nullable: true),
                    PosisiPelanggan = table.Column<int>(type: "INTEGER", nullable: true),
                    Kawasan = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plottings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plottings_AspNetUsers_SalesUserId",
                        column: x => x.SalesUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plottings_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TanggalSurvey = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SurveyorUserId = table.Column<string>(type: "TEXT", nullable: true),
                    JumlahKaryawan = table.Column<int>(type: "INTEGER", nullable: true),
                    JumlahShift = table.Column<int>(type: "INTEGER", nullable: true),
                    JamKerjaPerHari = table.Column<double>(type: "REAL", nullable: true),
                    HariPerMinggu = table.Column<int>(type: "INTEGER", nullable: true),
                    KebutuhanEnergi = table.Column<string>(type: "TEXT", nullable: false),
                    KapasitasNilai = table.Column<double>(type: "REAL", nullable: true),
                    KapasitasUnit = table.Column<string>(type: "TEXT", nullable: false),
                    PemakaianNilai = table.Column<double>(type: "REAL", nullable: true),
                    PemakaianUnit = table.Column<string>(type: "TEXT", nullable: false),
                    JumlahKebutuhanEnergi = table.Column<double>(type: "REAL", nullable: true),
                    PipaTerdekatJarakM = table.Column<double>(type: "REAL", nullable: true),
                    PipaTerdekatDiameter = table.Column<double>(type: "REAL", nullable: true),
                    PipaTerdekatTekanan = table.Column<double>(type: "REAL", nullable: true),
                    BahanBakarEksisting = table.Column<string>(type: "TEXT", nullable: false),
                    NamaPemasok = table.Column<string>(type: "TEXT", nullable: false),
                    KapasitasListrik = table.Column<double>(type: "REAL", nullable: true),
                    PemakaianListrik = table.Column<double>(type: "REAL", nullable: true),
                    RencanaPemanfaatanGas = table.Column<string>(type: "TEXT", nullable: false),
                    DeskripsiProsesProduksi = table.Column<string>(type: "TEXT", nullable: false),
                    KeteranganLain = table.Column<string>(type: "TEXT", nullable: false),
                    BebanPuncak1Mulai = table.Column<string>(type: "TEXT", nullable: false),
                    BebanPuncak1Selesai = table.Column<string>(type: "TEXT", nullable: false),
                    BebanPuncak2Mulai = table.Column<string>(type: "TEXT", nullable: false),
                    BebanPuncak2Selesai = table.Column<string>(type: "TEXT", nullable: false),
                    MinEfisiensiDiharapkanPct = table.Column<double>(type: "REAL", nullable: true),
                    WillingnessToPayUsdMmbtu = table.Column<double>(type: "REAL", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surveys_AspNetUsers_SurveyorUserId",
                        column: x => x.SurveyorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Surveys_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "A1Registrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TanggalRegistrasi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NamaPenanggungJawab = table.Column<string>(type: "TEXT", nullable: false),
                    JabatanPenanggungJawab = table.Column<string>(type: "TEXT", nullable: false),
                    BulanDimulai = table.Column<string>(type: "TEXT", nullable: false),
                    BasisKontrak = table.Column<int>(type: "INTEGER", nullable: true),
                    SkemaHarga = table.Column<int>(type: "INTEGER", nullable: true),
                    SegmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    KodeHarga = table.Column<string>(type: "TEXT", nullable: false),
                    HargaNilai = table.Column<double>(type: "REAL", nullable: true),
                    HargaCurrency = table.Column<int>(type: "INTEGER", nullable: true),
                    HargaUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    CapexAwal = table.Column<double>(type: "REAL", nullable: true),
                    MomSigasTersedia = table.Column<bool>(type: "INTEGER", nullable: false),
                    StatusBangunan = table.Column<int>(type: "INTEGER", nullable: true),
                    Sektor = table.Column<int>(type: "INTEGER", nullable: true),
                    ProduksiUtama = table.Column<string>(type: "TEXT", nullable: false),
                    JenisPeralatanGas = table.Column<string>(type: "TEXT", nullable: false),
                    TekananOperasiBarg = table.Column<double>(type: "REAL", nullable: true),
                    SignatureMethod = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_A1Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_A1Registrations_MasterData_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "MasterData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_A1Registrations_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeedStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    FeedCompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CapexFinal = table.Column<double>(type: "REAL", nullable: true),
                    PipaIndukPanjang = table.Column<double>(type: "REAL", nullable: true),
                    PipaIndukDiameter = table.Column<double>(type: "REAL", nullable: true),
                    PipaIndukUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    PipaServicePanjang = table.Column<double>(type: "REAL", nullable: true),
                    PipaServiceDiameter = table.Column<double>(type: "REAL", nullable: true),
                    PipaServiceUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    MrsSpecId = table.Column<int>(type: "INTEGER", nullable: true),
                    MeterSizeId = table.Column<int>(type: "INTEGER", nullable: true),
                    Tekanan = table.Column<double>(type: "REAL", nullable: true),
                    MaksFlowrate = table.Column<double>(type: "REAL", nullable: true),
                    MaksKapasitasMeterM3Jam = table.Column<double>(type: "REAL", nullable: true),
                    DurasiPelaksanaanBulan = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusRkap = table.Column<int>(type: "INTEGER", nullable: true),
                    SkemaPembayaran = table.Column<int>(type: "INTEGER", nullable: true),
                    JaminanStatus = table.Column<string>(type: "TEXT", nullable: false),
                    JaminanJenis = table.Column<string>(type: "TEXT", nullable: false),
                    JaminanPenerbit = table.Column<string>(type: "TEXT", nullable: false),
                    JaminanMasaBerlaku = table.Column<DateTime>(type: "TEXT", nullable: true),
                    KetersediaanPasokanBbtud = table.Column<double>(type: "REAL", nullable: true),
                    AnalisisKomersial = table.Column<string>(type: "TEXT", nullable: false),
                    AnalisisKompetitor = table.Column<string>(type: "TEXT", nullable: false),
                    RadiusKompetitorKm = table.Column<double>(type: "REAL", nullable: true),
                    Kesimpulan = table.Column<string>(type: "TEXT", nullable: false),
                    EvaluatedById = table.Column<string>(type: "TEXT", nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolEvaluations_AspNetUsers_EvaluatedById",
                        column: x => x.EvaluatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NolEvaluations_MasterData_MeterSizeId",
                        column: x => x.MeterSizeId,
                        principalTable: "MasterData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NolEvaluations_MasterData_MrsSpecId",
                        column: x => x.MrsSpecId,
                        principalTable: "MasterData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NolEvaluations_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    NomorNotaDinas = table.Column<string>(type: "TEXT", nullable: false),
                    RegistrationType = table.Column<int>(type: "INTEGER", nullable: true),
                    SamaDenganA1 = table.Column<bool>(type: "INTEGER", nullable: false),
                    BulanDimulai = table.Column<string>(type: "TEXT", nullable: false),
                    BasisKontrak = table.Column<int>(type: "INTEGER", nullable: true),
                    SkemaHarga = table.Column<int>(type: "INTEGER", nullable: true),
                    SegmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    KodeHarga = table.Column<string>(type: "TEXT", nullable: false),
                    HargaNilai = table.Column<double>(type: "REAL", nullable: true),
                    HargaCurrency = table.Column<int>(type: "INTEGER", nullable: true),
                    HargaUnit = table.Column<int>(type: "INTEGER", nullable: true),
                    AlasanKontrakBersyarat = table.Column<string>(type: "TEXT", nullable: false),
                    NamaPimpinanPerusahaan = table.Column<string>(type: "TEXT", nullable: false),
                    JangkaWaktuKontrak = table.Column<string>(type: "TEXT", nullable: false),
                    Lampiran17 = table.Column<string>(type: "TEXT", nullable: false),
                    CapexPreGr3 = table.Column<double>(type: "REAL", nullable: true),
                    BiayaPenyambunganReguler = table.Column<double>(type: "REAL", nullable: true),
                    BiayaPenyambunganExtra = table.Column<double>(type: "REAL", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolRequests_MasterData_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "MasterData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NolRequests_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolIssuanceConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NolIssuanceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Isi = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolIssuanceConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolIssuanceConditions_NolIssuances_NolIssuanceId",
                        column: x => x.NolIssuanceId,
                        principalTable: "NolIssuances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolIssuanceTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NolIssuanceId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodeMulai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PeriodeSelesai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RataRata = table.Column<double>(type: "REAL", nullable: true),
                    Minimum = table.Column<double>(type: "REAL", nullable: true),
                    Maksimum = table.Column<double>(type: "REAL", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolIssuanceTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolIssuanceTerms_NolIssuances_NolIssuanceId",
                        column: x => x.NolIssuanceId,
                        principalTable: "NolIssuances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyEquipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SurveyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Jenis = table.Column<string>(type: "TEXT", nullable: false),
                    Kapasitas = table.Column<double>(type: "REAL", nullable: true),
                    JamPerHari = table.Column<double>(type: "REAL", nullable: true),
                    HariPerMinggu = table.Column<int>(type: "INTEGER", nullable: true),
                    BahanBakar = table.Column<string>(type: "TEXT", nullable: false),
                    HargaBahanBakar = table.Column<double>(type: "REAL", nullable: true),
                    KonsumsiPerBulan = table.Column<double>(type: "REAL", nullable: true),
                    KonversiKeGas = table.Column<double>(type: "REAL", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyEquipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyEquipment_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyMarkets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SurveyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nama = table.Column<string>(type: "TEXT", nullable: false),
                    IsEkspor = table.Column<bool>(type: "INTEGER", nullable: false),
                    PersentasePct = table.Column<double>(type: "REAL", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyMarkets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyMarkets_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SurveyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nama = table.Column<string>(type: "TEXT", nullable: false),
                    Kapasitas = table.Column<double>(type: "REAL", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyProducts_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyRawMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SurveyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nama = table.Column<string>(type: "TEXT", nullable: false),
                    IsImpor = table.Column<bool>(type: "INTEGER", nullable: false),
                    NegaraAsal = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyRawMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyRawMaterials_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "A1UsagePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    A1RegistrationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodeMulai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PeriodeSelesai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RataRata = table.Column<double>(type: "REAL", nullable: true),
                    Minimum = table.Column<double>(type: "REAL", nullable: true),
                    Maksimum = table.Column<double>(type: "REAL", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_A1UsagePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_A1UsagePeriods_A1Registrations_A1RegistrationId",
                        column: x => x.A1RegistrationId,
                        principalTable: "A1Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolEvaluationScenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NolEvaluationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    IrrPct = table.Column<double>(type: "REAL", nullable: true),
                    Npv = table.Column<double>(type: "REAL", nullable: true),
                    PaybackYears = table.Column<double>(type: "REAL", nullable: true),
                    HasilAnalisis = table.Column<string>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolEvaluationScenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolEvaluationScenarios_NolEvaluations_NolEvaluationId",
                        column: x => x.NolEvaluationId,
                        principalTable: "NolEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolRequestPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NolRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeriodeMulai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PeriodeSelesai = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RataRata = table.Column<double>(type: "REAL", nullable: true),
                    Minimum = table.Column<double>(type: "REAL", nullable: true),
                    Maksimum = table.Column<double>(type: "REAL", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolRequestPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolRequestPeriods_NolRequests_NolRequestId",
                        column: x => x.NolRequestId,
                        principalTable: "NolRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NolRequestReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NolRequestId = table.Column<int>(type: "INTEGER", nullable: false),
                    Judul = table.Column<string>(type: "TEXT", nullable: false),
                    Nomor = table.Column<string>(type: "TEXT", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NolRequestReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NolRequestReferences_NolRequests_NolRequestId",
                        column: x => x.NolRequestId,
                        principalTable: "NolRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_A1Registrations_SegmentId",
                table: "A1Registrations",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_A1Registrations_SubscriptionId",
                table: "A1Registrations",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_A1UsagePeriods_A1RegistrationId",
                table: "A1UsagePeriods",
                column: "A1RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakGlassGrants_ExpiresAt",
                table: "BreakGlassGrants",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BreakGlassGrants_GrantedById",
                table: "BreakGlassGrants",
                column: "GrantedById");

            migrationBuilder.CreateIndex(
                name: "IX_BreakGlassGrants_GrantedToUserId",
                table: "BreakGlassGrants",
                column: "GrantedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakGlassGrants_SubscriptionId",
                table: "BreakGlassGrants",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContacts_SubscriptionId",
                table: "CompanyContacts",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MasterData_Category_SortOrder",
                table: "MasterData",
                columns: new[] { "Category", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_NolEvaluations_EvaluatedById",
                table: "NolEvaluations",
                column: "EvaluatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NolEvaluations_MeterSizeId",
                table: "NolEvaluations",
                column: "MeterSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_NolEvaluations_MrsSpecId",
                table: "NolEvaluations",
                column: "MrsSpecId");

            migrationBuilder.CreateIndex(
                name: "IX_NolEvaluations_SubscriptionId",
                table: "NolEvaluations",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NolEvaluationScenarios_NolEvaluationId",
                table: "NolEvaluationScenarios",
                column: "NolEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_NolIssuanceConditions_NolIssuanceId",
                table: "NolIssuanceConditions",
                column: "NolIssuanceId");

            migrationBuilder.CreateIndex(
                name: "IX_NolIssuances_SignedById",
                table: "NolIssuances",
                column: "SignedById");

            migrationBuilder.CreateIndex(
                name: "IX_NolIssuances_SubscriptionId",
                table: "NolIssuances",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NolIssuanceTerms_NolIssuanceId",
                table: "NolIssuanceTerms",
                column: "NolIssuanceId");

            migrationBuilder.CreateIndex(
                name: "IX_NolRequestPeriods_NolRequestId",
                table: "NolRequestPeriods",
                column: "NolRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NolRequestReferences_NolRequestId",
                table: "NolRequestReferences",
                column: "NolRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_NolRequests_SegmentId",
                table: "NolRequests",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_NolRequests_SubscriptionId",
                table: "NolRequests",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plottings_SalesUserId",
                table: "Plottings",
                column: "SalesUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Plottings_SubscriptionId",
                table: "Plottings",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyEquipment_SurveyId",
                table: "SurveyEquipment",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyMarkets_SurveyId",
                table: "SurveyMarkets",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyProducts_SurveyId",
                table: "SurveyProducts",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyRawMaterials_SurveyId",
                table: "SurveyRawMaterials",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_SubscriptionId",
                table: "Surveys",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_SurveyorUserId",
                table: "Surveys",
                column: "SurveyorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "A1UsagePeriods");

            migrationBuilder.DropTable(
                name: "BreakGlassGrants");

            migrationBuilder.DropTable(
                name: "CompanyContacts");

            migrationBuilder.DropTable(
                name: "NolEvaluationScenarios");

            migrationBuilder.DropTable(
                name: "NolIssuanceConditions");

            migrationBuilder.DropTable(
                name: "NolIssuanceTerms");

            migrationBuilder.DropTable(
                name: "NolRequestPeriods");

            migrationBuilder.DropTable(
                name: "NolRequestReferences");

            migrationBuilder.DropTable(
                name: "Plottings");

            migrationBuilder.DropTable(
                name: "SurveyEquipment");

            migrationBuilder.DropTable(
                name: "SurveyMarkets");

            migrationBuilder.DropTable(
                name: "SurveyProducts");

            migrationBuilder.DropTable(
                name: "SurveyRawMaterials");

            migrationBuilder.DropTable(
                name: "A1Registrations");

            migrationBuilder.DropTable(
                name: "NolEvaluations");

            migrationBuilder.DropTable(
                name: "NolIssuances");

            migrationBuilder.DropTable(
                name: "NolRequests");

            migrationBuilder.DropTable(
                name: "Surveys");

            migrationBuilder.DropTable(
                name: "MasterData");
        }
    }
}
