using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusEventAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "status_event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_stage = table.Column<byte>(type: "smallint", nullable: true),
                    to_stage = table.Column<byte>(type: "smallint", nullable: false),
                    from_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    to_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_event", x => x.id);
                    table.CheckConstraint("ck_status_event_comment_required_on_revisi_tolak", "(action not in ('Revisi', 'Tolak')) or (comment is not null)");
                    table.ForeignKey(
                        name: "fk_status_event_app_user_actor_id",
                        column: x => x.actor_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_status_event_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_status_event_actor_id",
                table: "status_event",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_status_event_company_id_occurred_at",
                table: "status_event",
                columns: new[] { "company_id", "occurred_at" });

            // Append-only, at the database level — application discipline
            // alone is not acceptable for a table backing signed commercial
            // documents. docs/design/data-model.md#status_event.
            migrationBuilder.Sql("""
                create function reject_status_event_update_delete() returns trigger as $$
                begin
                    raise exception 'status_event is append-only: % is not permitted', tg_op;
                end;
                $$ language plpgsql;

                create trigger status_event_no_update
                    before update on status_event
                    for each row execute function reject_status_event_update_delete();

                create trigger status_event_no_delete
                    before delete on status_event
                    for each row execute function reject_status_event_update_delete();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                drop trigger if exists status_event_no_delete on status_event;
                drop trigger if exists status_event_no_update on status_event;
                drop function if exists reject_status_event_update_delete();
                """);

            migrationBuilder.DropTable(
                name: "status_event");
        }
    }
}
