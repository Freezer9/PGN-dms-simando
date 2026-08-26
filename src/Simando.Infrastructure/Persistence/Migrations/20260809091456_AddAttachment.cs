using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attachment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attachable_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    attachable_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    filename = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    storage_provider = table.Column<string>(type: "text", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attachment", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attachment_attachable_type_attachable_id",
                table: "attachment",
                columns: new[] { "attachable_type", "attachable_id" });

            migrationBuilder.CreateIndex(
                name: "ix_attachment_company_id",
                table: "attachment",
                column: "company_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attachment");
        }
    }
}
