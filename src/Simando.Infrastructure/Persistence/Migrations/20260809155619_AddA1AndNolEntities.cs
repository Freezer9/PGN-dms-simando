using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddA1AndNolEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "a1_registration",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tanggal_registrasi = table.Column<DateOnly>(type: "date", nullable: true),
                    registrasi_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nama_penanggung_jawab = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    jabatan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bulan_dimulai = table.Column<DateOnly>(type: "date", nullable: true),
                    basis_kontrak = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    skema_harga = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    segment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kode_harga = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    harga_nilai = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    harga_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    harga_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    capex_awal = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    mom_sigas_tersedia = table.Column<bool>(type: "boolean", nullable: false),
                    status_bangunan = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    sektor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    produksi_utama = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    jenis_peralatan_gas = table.Column<string>(type: "text", nullable: true),
                    tekanan_operasi_barg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    signed_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    signature_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_a1_registration", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_a1_registration_attachments_signed_document_id",
                        column: x => x.signed_document_id,
                        principalTable: "attachment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_a1_registration_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_a1_registration_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evaluation_resume",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_evaluation_nol_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    generated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attachment_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evaluation_resume", x => x.id);
                    table.ForeignKey(
                        name: "fk_evaluation_resume_app_user_generated_by_user_id",
                        column: x => x.generated_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_evaluation_resume_attachment_attachment_id",
                        column: x => x.attachment_id,
                        principalTable: "attachment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nol_request",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nomor_nota_dinas = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    registration_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sama_dengan_a1 = table.Column<bool>(type: "boolean", nullable: false),
                    bulan_dimulai = table.Column<DateOnly>(type: "date", nullable: true),
                    basis_kontrak = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    skema_harga = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    segment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kode_harga = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    harga_nilai = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    harga_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    harga_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    alasan_kontrak_bersyarat = table.Column<string>(type: "text", nullable: true),
                    nama_pimpinan_perusahaan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    jangka_waktu_kontrak = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    capex_pre_gr3 = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    biaya_penyambungan_reguler = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    biaya_penyambungan_extra = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    workflow_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_request", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_nol_request_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_nol_request_segments_segment_id",
                        column: x => x.segment_id,
                        principalTable: "segment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nol_request_workflow_instances_workflow_instance_id",
                        column: x => x.workflow_instance_id,
                        principalTable: "workflow_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "a1_usage_period",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    a1_registration_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periode_mulai = table.Column<DateOnly>(type: "date", nullable: false),
                    periode_selesai = table.Column<DateOnly>(type: "date", nullable: false),
                    rata_rata = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    minimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    maksimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_a1_usage_period", x => x.id);
                    table.ForeignKey(
                        name: "fk_a1_usage_period_a1_registration_a1registration_company_id",
                        column: x => x.a1_registration_company_id,
                        principalTable: "a1_registration",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_evaluation",
                columns: table => new
                {
                    nol_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feed_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    feed_completed_at = table.Column<DateOnly>(type: "date", nullable: true),
                    capex_final = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_induk_panjang_m = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_induk_diameter = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_induk_diameter_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    pipa_service_panjang_m = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_service_diameter = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_service_diameter_unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    spesifikasi_mrs = table.Column<string>(type: "text", nullable: true),
                    g_size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tekanan = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    maks_flowrate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    maks_kapasitas_meter_m3_jam = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    durasi_pelaksanaan_bulan = table.Column<short>(type: "smallint", nullable: true),
                    status_rkap = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    skema_pembayaran = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    jaminan_status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    jaminan_jenis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    jaminan_masa_berlaku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    jaminan_penerbit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ketersediaan_pasokan_bbtud = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    analisis_komersial = table.Column<string>(type: "text", nullable: true),
                    analisis_kompetitor = table.Column<string>(type: "text", nullable: true),
                    kesimpulan = table.Column<string>(type: "text", nullable: true),
                    radius_kompetitor_km = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    evaluated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_evaluation", x => x.nol_request_id);
                    table.ForeignKey(
                        name: "fk_nol_evaluation_app_user_evaluated_by",
                        column: x => x.evaluated_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nol_evaluation_nol_request_nol_request_id",
                        column: x => x.nol_request_id,
                        principalTable: "nol_request",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_issuance",
                columns: table => new
                {
                    nol_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nomor_nota_dinas = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    kontrak_bersyarat = table.Column<List<string>>(type: "text[]", nullable: false),
                    berlaku_sejak = table.Column<DateOnly>(type: "date", nullable: true),
                    berlaku_sampai = table.Column<DateOnly>(type: "date", nullable: true),
                    signed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    signed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_issuance", x => x.nol_request_id);
                    table.ForeignKey(
                        name: "fk_nol_issuance_app_user_signed_by_user_id",
                        column: x => x.signed_by_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nol_issuance_attachment_document_id",
                        column: x => x.document_id,
                        principalTable: "attachment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nol_issuance_nol_request_nol_request_id",
                        column: x => x.nol_request_id,
                        principalTable: "nol_request",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_request_daily",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_request_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hari = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    min = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    max = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_request_daily", x => x.id);
                    table.ForeignKey(
                        name: "fk_nol_request_daily_nol_request_nol_request_company_id",
                        column: x => x.nol_request_company_id,
                        principalTable: "nol_request",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_request_period",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_request_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periode_mulai = table.Column<DateOnly>(type: "date", nullable: false),
                    periode_selesai = table.Column<DateOnly>(type: "date", nullable: false),
                    rata_rata = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    kontrak_minimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    kontrak_maksimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_request_period", x => x.id);
                    table.ForeignKey(
                        name: "fk_nol_request_period_nol_request_nol_request_company_id",
                        column: x => x.nol_request_company_id,
                        principalTable: "nol_request",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_request_reference",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_request_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_document_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_request_reference", x => x.id);
                    table.ForeignKey(
                        name: "fk_nol_request_reference_nol_request_nol_request_company_id",
                        column: x => x.nol_request_company_id,
                        principalTable: "nol_request",
                        principalColumn: "company_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_nol_request_reference_reference_documents_reference_documen",
                        column: x => x.reference_document_id,
                        principalTable: "reference_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nol_evaluation_scenario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_evaluation_nol_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    irr_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    npv = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    payback_years = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    hasil_analisis = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_evaluation_scenario", x => x.id);
                    table.ForeignKey(
                        name: "fk_nol_evaluation_scenario_nol_evaluation_nol_evaluation_nol_r",
                        column: x => x.nol_evaluation_nol_request_id,
                        principalTable: "nol_evaluation",
                        principalColumn: "nol_request_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nol_issuance_approved_term",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nol_issuance_nol_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    periode_mulai = table.Column<DateOnly>(type: "date", nullable: false),
                    periode_selesai = table.Column<DateOnly>(type: "date", nullable: false),
                    rata_rata = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    kontrak_minimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    kontrak_maksimum = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nol_issuance_approved_term", x => x.id);
                    table.ForeignKey(
                        name: "fk_nol_issuance_approved_term_nol_issuance_nol_issuance_nol_re",
                        column: x => x.nol_issuance_nol_request_id,
                        principalTable: "nol_issuance",
                        principalColumn: "nol_request_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_a1_registration_segment_id",
                table: "a1_registration",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "ix_a1_registration_signed_document_id",
                table: "a1_registration",
                column: "signed_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_a1_usage_period_a1registration_company_id",
                table: "a1_usage_period",
                column: "a1_registration_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_resume_attachment_id",
                table: "evaluation_resume",
                column: "attachment_id");

            migrationBuilder.CreateIndex(
                name: "ix_evaluation_resume_generated_by_user_id",
                table: "evaluation_resume",
                column: "generated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_evaluation_evaluated_by",
                table: "nol_evaluation",
                column: "evaluated_by");

            migrationBuilder.CreateIndex(
                name: "ix_nol_evaluation_scenario_nol_evaluation_nol_request_id",
                table: "nol_evaluation_scenario",
                column: "nol_evaluation_nol_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_issuance_document_id",
                table: "nol_issuance",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_issuance_signed_by_user_id",
                table: "nol_issuance",
                column: "signed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_issuance_approved_term_nol_issuance_nol_request_id",
                table: "nol_issuance_approved_term",
                column: "nol_issuance_nol_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_segment_id",
                table: "nol_request",
                column: "segment_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_workflow_instance_id",
                table: "nol_request",
                column: "workflow_instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_daily_nol_request_company_id",
                table: "nol_request_daily",
                column: "nol_request_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_period_nol_request_company_id",
                table: "nol_request_period",
                column: "nol_request_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_reference_nol_request_company_id",
                table: "nol_request_reference",
                column: "nol_request_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_nol_request_reference_reference_document_id",
                table: "nol_request_reference",
                column: "reference_document_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "a1_usage_period");

            migrationBuilder.DropTable(
                name: "evaluation_resume");

            migrationBuilder.DropTable(
                name: "nol_evaluation_scenario");

            migrationBuilder.DropTable(
                name: "nol_issuance_approved_term");

            migrationBuilder.DropTable(
                name: "nol_request_daily");

            migrationBuilder.DropTable(
                name: "nol_request_period");

            migrationBuilder.DropTable(
                name: "nol_request_reference");

            migrationBuilder.DropTable(
                name: "a1_registration");

            migrationBuilder.DropTable(
                name: "nol_evaluation");

            migrationBuilder.DropTable(
                name: "nol_issuance");

            migrationBuilder.DropTable(
                name: "nol_request");
        }
    }
}
