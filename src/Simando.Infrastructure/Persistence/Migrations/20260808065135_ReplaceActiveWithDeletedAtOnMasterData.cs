using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceActiveWithDeletedAtOnMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_village_district_id_bps_code",
                table: "village");

            migrationBuilder.DropIndex(
                name: "ix_unit_of_measure_code",
                table: "unit_of_measure");

            migrationBuilder.DropIndex(
                name: "ix_segment_name",
                table: "segment");

            migrationBuilder.DropIndex(
                name: "ix_regency_province_id_bps_code",
                table: "regency");

            migrationBuilder.DropIndex(
                name: "ix_reason_category_name",
                table: "reason_category");

            migrationBuilder.DropIndex(
                name: "ix_province_bps_code",
                table: "province");

            migrationBuilder.DropIndex(
                name: "ix_mrs_spec_name",
                table: "mrs_spec");

            migrationBuilder.DropIndex(
                name: "ix_meter_size_g_size",
                table: "meter_size");

            migrationBuilder.DropIndex(
                name: "ix_industry_type_name",
                table: "industry_type");

            migrationBuilder.DropIndex(
                name: "ix_fuel_type_name",
                table: "fuel_type");

            migrationBuilder.DropIndex(
                name: "ix_district_regency_id_bps_code",
                table: "district");

            migrationBuilder.DropIndex(
                name: "ix_country_iso_code",
                table: "country");

            migrationBuilder.DropColumn(
                name: "active",
                table: "village");

            migrationBuilder.DropColumn(
                name: "active",
                table: "unit_of_measure");

            migrationBuilder.DropColumn(
                name: "active",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "active",
                table: "regency");

            migrationBuilder.DropColumn(
                name: "active",
                table: "reason_category");

            migrationBuilder.DropColumn(
                name: "active",
                table: "province");

            migrationBuilder.DropColumn(
                name: "active",
                table: "mrs_spec");

            migrationBuilder.DropColumn(
                name: "active",
                table: "meter_size");

            migrationBuilder.DropColumn(
                name: "active",
                table: "industry_type");

            migrationBuilder.DropColumn(
                name: "active",
                table: "fuel_type");

            migrationBuilder.DropColumn(
                name: "active",
                table: "district");

            migrationBuilder.DropColumn(
                name: "active",
                table: "country");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "village",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "unit_of_measure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "segment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "regency",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "reason_category",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "province",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "mrs_spec",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "meter_size",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "industry_type",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "fuel_type",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "district",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "country",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_village_district_id_bps_code",
                table: "village",
                columns: new[] { "district_id", "bps_code" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_unit_of_measure_code",
                table: "unit_of_measure",
                column: "code",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_segment_name",
                table: "segment",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_regency_province_id_bps_code",
                table: "regency",
                columns: new[] { "province_id", "bps_code" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_reason_category_name",
                table: "reason_category",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_province_bps_code",
                table: "province",
                column: "bps_code",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_mrs_spec_name",
                table: "mrs_spec",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_meter_size_g_size",
                table: "meter_size",
                column: "g_size",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_industry_type_name",
                table: "industry_type",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_type_name",
                table: "fuel_type",
                column: "name",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_district_regency_id_bps_code",
                table: "district",
                columns: new[] { "regency_id", "bps_code" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_country_iso_code",
                table: "country",
                column: "iso_code",
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_village_district_id_bps_code",
                table: "village");

            migrationBuilder.DropIndex(
                name: "ix_unit_of_measure_code",
                table: "unit_of_measure");

            migrationBuilder.DropIndex(
                name: "ix_segment_name",
                table: "segment");

            migrationBuilder.DropIndex(
                name: "ix_regency_province_id_bps_code",
                table: "regency");

            migrationBuilder.DropIndex(
                name: "ix_reason_category_name",
                table: "reason_category");

            migrationBuilder.DropIndex(
                name: "ix_province_bps_code",
                table: "province");

            migrationBuilder.DropIndex(
                name: "ix_mrs_spec_name",
                table: "mrs_spec");

            migrationBuilder.DropIndex(
                name: "ix_meter_size_g_size",
                table: "meter_size");

            migrationBuilder.DropIndex(
                name: "ix_industry_type_name",
                table: "industry_type");

            migrationBuilder.DropIndex(
                name: "ix_fuel_type_name",
                table: "fuel_type");

            migrationBuilder.DropIndex(
                name: "ix_district_regency_id_bps_code",
                table: "district");

            migrationBuilder.DropIndex(
                name: "ix_country_iso_code",
                table: "country");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "village");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "unit_of_measure");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "regency");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "reason_category");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "province");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "mrs_spec");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "meter_size");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "industry_type");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "fuel_type");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "district");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "country");

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "village",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "unit_of_measure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "segment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "regency",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "reason_category",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "province",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "mrs_spec",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "meter_size",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "industry_type",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "fuel_type",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "district",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "country",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_village_district_id_bps_code",
                table: "village",
                columns: new[] { "district_id", "bps_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unit_of_measure_code",
                table: "unit_of_measure",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_segment_name",
                table: "segment",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regency_province_id_bps_code",
                table: "regency",
                columns: new[] { "province_id", "bps_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reason_category_name",
                table: "reason_category",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_province_bps_code",
                table: "province",
                column: "bps_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mrs_spec_name",
                table: "mrs_spec",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_meter_size_g_size",
                table: "meter_size",
                column: "g_size",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_industry_type_name",
                table: "industry_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_type_name",
                table: "fuel_type",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_district_regency_id_bps_code",
                table: "district",
                columns: new[] { "regency_id", "bps_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_country_iso_code",
                table: "country",
                column: "iso_code",
                unique: true);
        }
    }
}
