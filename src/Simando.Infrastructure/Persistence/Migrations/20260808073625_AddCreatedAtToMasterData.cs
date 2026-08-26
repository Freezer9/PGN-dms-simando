using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "village",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "unit_of_measure",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "segment",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "regency",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "reason_category",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "province",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "mrs_spec",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "meter_size",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "industry_type",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "fuel_type",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "district",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "country",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "village");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "unit_of_measure");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "segment");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "regency");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "reason_category");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "province");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "mrs_spec");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "meter_size");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "industry_type");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "fuel_type");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "district");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "country");
        }
    }
}
