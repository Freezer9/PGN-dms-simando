using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterDataFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    iso_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_country", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_number_counter",
                columns: table => new
                {
                    document_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    scope_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period_key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    next_seq = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_number_counter", x => new { x.document_type, x.scope_key, x.period_key });
                });

            migrationBuilder.CreateTable(
                name: "fuel_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "industry_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    contoh_produk = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_industry_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meter_size",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    g_size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nominal_flow = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    max_flow = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    pressure_rating = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meter_size", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mrs_spec",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mrs_spec", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "province",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bps_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_province", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reason_category",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reason_category", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reference_document",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                    blob_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reference_document", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "segment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_segment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unit_of_measure",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dimension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_of_measure", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regency",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bps_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regency", x => x.id);
                    table.ForeignKey(
                        name: "fk_regency_province_province_id",
                        column: x => x.province_id,
                        principalTable: "province",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unit_set_member",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    set_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_set_member", x => x.id);
                    table.ForeignKey(
                        name: "fk_unit_set_member_unit_of_measure_unit_id",
                        column: x => x.unit_id,
                        principalTable: "unit_of_measure",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "district",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    regency_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bps_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_district", x => x.id);
                    table.ForeignKey(
                        name: "fk_district_regencies_regency_id",
                        column: x => x.regency_id,
                        principalTable: "regency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "village",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    district_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bps_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_village", x => x.id);
                    table.ForeignKey(
                        name: "fk_village_district_district_id",
                        column: x => x.district_id,
                        principalTable: "district",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_country_iso_code",
                table: "country",
                column: "iso_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_district_regency_id_bps_code",
                table: "district",
                columns: new[] { "regency_id", "bps_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_type_name",
                table: "fuel_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_industry_type_name",
                table: "industry_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meter_size_g_size",
                table: "meter_size",
                column: "g_size",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mrs_spec_name",
                table: "mrs_spec",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_province_bps_code",
                table: "province",
                column: "bps_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reason_category_name",
                table: "reason_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reference_document_name_version",
                table: "reference_document",
                columns: new[] { "name", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regency_province_id_bps_code",
                table: "regency",
                columns: new[] { "province_id", "bps_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_segment_name",
                table: "segment",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unit_of_measure_code",
                table: "unit_of_measure",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unit_set_member_set_code_unit_id",
                table: "unit_set_member",
                columns: new[] { "set_code", "unit_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unit_set_member_unit_id",
                table: "unit_set_member",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_village_district_id_bps_code",
                table: "village",
                columns: new[] { "district_id", "bps_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropTable(
                name: "document_number_counter");

            migrationBuilder.DropTable(
                name: "fuel_type");

            migrationBuilder.DropTable(
                name: "industry_type");

            migrationBuilder.DropTable(
                name: "meter_size");

            migrationBuilder.DropTable(
                name: "mrs_spec");

            migrationBuilder.DropTable(
                name: "reason_category");

            migrationBuilder.DropTable(
                name: "reference_document");

            migrationBuilder.DropTable(
                name: "segment");

            migrationBuilder.DropTable(
                name: "unit_set_member");

            migrationBuilder.DropTable(
                name: "village");

            migrationBuilder.DropTable(
                name: "unit_of_measure");

            migrationBuilder.DropTable(
                name: "district");

            migrationBuilder.DropTable(
                name: "regency");

            migrationBuilder.DropTable(
                name: "province");
        }
    }
}
