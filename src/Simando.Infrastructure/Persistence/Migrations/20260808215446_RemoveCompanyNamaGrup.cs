using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyNamaGrup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nama_grup",
                table: "company");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nama_grup",
                table: "company",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
