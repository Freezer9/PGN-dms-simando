using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyDirectoryFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "company_nomor_seq");

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nomor_seq = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('company_nomor_seq')"),
                    nomor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nama_perusahaan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nama_grup = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    website = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    village_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alamat = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<Point>(type: "geography(Point,4326)", nullable: true),
                    industry_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    npwp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    kode_pos = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    telp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    fax = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_stage = table.Column<byte>(type: "smallint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_app_user_created_by",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_area_area_id",
                        column: x => x.area_id,
                        principalTable: "area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_industry_types_industry_type_id",
                        column: x => x.industry_type_id,
                        principalTable: "industry_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_villages_village_id",
                        column: x => x.village_id,
                        principalTable: "village",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "company_contact",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nama = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    jabatan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    no_hp = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    linkedin = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    instagram = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    facebook = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_contact", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_contact_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plotting",
                columns: table => new
                {
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sales_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posisi_pelanggan = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kawasan = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plotting", x => x.company_id);
                    table.ForeignKey(
                        name: "fk_plotting_app_user_sales_user_id",
                        column: x => x.sales_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plotting_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_area_id_current_stage_status",
                table: "company",
                columns: new[] { "area_id", "current_stage", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_company_created_by",
                table: "company",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_company_industry_type_id",
                table: "company",
                column: "industry_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_location",
                table: "company",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "ix_company_nomor",
                table: "company",
                column: "nomor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_nomor_seq",
                table: "company",
                column: "nomor_seq",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_village_id",
                table: "company",
                column: "village_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_contact_company_id",
                table: "company_contact",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_plotting_sales_user_id",
                table: "plotting",
                column: "sales_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_contact");

            migrationBuilder.DropTable(
                name: "plotting");

            migrationBuilder.DropTable(
                name: "company");

            migrationBuilder.DropSequence(
                name: "company_nomor_seq");
        }
    }
}
