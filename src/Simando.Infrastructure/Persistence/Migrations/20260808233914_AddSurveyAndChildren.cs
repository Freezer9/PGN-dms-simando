using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyAndChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "survey",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tanggal_survey = table.Column<DateOnly>(type: "date", nullable: true),
                    surveyor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    jumlah_karyawan = table.Column<int>(type: "integer", nullable: true),
                    jumlah_shift = table.Column<short>(type: "smallint", nullable: true),
                    jam_kerja_per_hari = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    hari_per_minggu = table.Column<short>(type: "smallint", nullable: true),
                    kebutuhan_energi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    kebutuhan_energi_lainnya = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    kapasitas_nilai = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    kapasitas_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pemakaian_nilai = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pemakaian_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pipa_terdekat_jarak_m = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_terdekat_diameter = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pipa_terdekat_tekanan = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    bahan_bakar_eksisting = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nama_pemasok = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    kapasitas_listrik_kw = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    pemakaian_listrik_kwh = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    rencana_pemanfaatan_gas = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deskripsi_proses_produksi = table.Column<string>(type: "text", nullable: true),
                    min_efisiensi_diharapkan_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    willingness_to_pay_usd_mmbtu = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    keterangan_lain = table.Column<string>(type: "text", nullable: true),
                    jumlah_kebutuhan_energi = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_survey_app_user_surveyor_user_id",
                        column: x => x.surveyor_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_survey_units_of_measure_kapasitas_unit_id",
                        column: x => x.kapasitas_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_units_of_measure_pemakaian_unit_id",
                        column: x => x.pemakaian_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "survey_equipment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    jenis_peralatan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kapasitas = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    kapasitas_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    jam_per_hari = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    hari_per_minggu = table.Column<short>(type: "smallint", nullable: true),
                    fuel_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    harga_bahan_bakar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    konsumsi_per_bulan = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    konsumsi_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    konversi_ke_gas = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_equipment", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_equipment_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_survey_equipment_fuel_type_fuel_type_id",
                        column: x => x.fuel_type_id,
                        principalTable: "fuel_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_equipment_units_of_measure_kapasitas_unit_id",
                        column: x => x.kapasitas_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_equipment_units_of_measure_konsumsi_unit_id",
                        column: x => x.konsumsi_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "survey_market",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bahan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    asal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    volume = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    satuan_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_market", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_market_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_survey_market_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_market_units_of_measure_satuan_unit_id",
                        column: x => x.satuan_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "survey_product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    produk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kapasitas = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    harga_produk = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    catatan = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_product", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_product_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_raw_material",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bahan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    asal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    volume = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    satuan_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_survey_raw_material", x => x.id);
                    table.ForeignKey(
                        name: "fk_survey_raw_material_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_survey_raw_material_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_survey_raw_material_units_of_measure_satuan_unit_id",
                        column: x => x.satuan_unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_survey_kapasitas_unit_id",
                table: "survey",
                column: "kapasitas_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_pemakaian_unit_id",
                table: "survey",
                column: "pemakaian_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_surveyor_user_id",
                table: "survey",
                column: "surveyor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_equipment_company_id",
                table: "survey_equipment",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_equipment_fuel_type_id",
                table: "survey_equipment",
                column: "fuel_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_equipment_kapasitas_unit_id",
                table: "survey_equipment",
                column: "kapasitas_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_equipment_konsumsi_unit_id",
                table: "survey_equipment",
                column: "konsumsi_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_market_company_id",
                table: "survey_market",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_market_country_id",
                table: "survey_market",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_market_satuan_unit_id",
                table: "survey_market",
                column: "satuan_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_product_company_id",
                table: "survey_product",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_raw_material_company_id",
                table: "survey_raw_material",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_raw_material_country_id",
                table: "survey_raw_material",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "ix_survey_raw_material_satuan_unit_id",
                table: "survey_raw_material",
                column: "satuan_unit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "survey");

            migrationBuilder.DropTable(
                name: "survey_equipment");

            migrationBuilder.DropTable(
                name: "survey_market");

            migrationBuilder.DropTable(
                name: "survey_product");

            migrationBuilder.DropTable(
                name: "survey_raw_material");
        }
    }
}
