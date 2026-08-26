using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBreakGlassAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "break_glass_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_break_glass_access", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_break_glass_access_user_id_company_id_expires_at",
                table: "break_glass_access",
                columns: new[] { "user_id", "company_id", "expires_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "break_glass_access");
        }
    }
}
