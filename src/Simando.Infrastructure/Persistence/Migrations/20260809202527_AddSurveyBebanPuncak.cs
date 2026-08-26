using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyBebanPuncak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "beban_puncak1_mulai",
                table: "survey",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "beban_puncak1_selesai",
                table: "survey",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "beban_puncak2_mulai",
                table: "survey",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "beban_puncak2_selesai",
                table: "survey",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "beban_puncak1_mulai",
                table: "survey");

            migrationBuilder.DropColumn(
                name: "beban_puncak1_selesai",
                table: "survey");

            migrationBuilder.DropColumn(
                name: "beban_puncak2_mulai",
                table: "survey");

            migrationBuilder.DropColumn(
                name: "beban_puncak2_selesai",
                table: "survey");
        }
    }
}
