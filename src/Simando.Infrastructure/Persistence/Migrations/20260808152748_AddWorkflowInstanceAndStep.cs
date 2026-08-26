using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simando.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowInstanceAndStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "workflow_step_id",
                table: "status_event",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "workflow_instance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_count = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    final_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_instance", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_instance_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_step",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_order = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    assigned_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    acted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    acted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_step", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_step_app_user_acted_by",
                        column: x => x.acted_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_workflow_step_app_user_assigned_user_id",
                        column: x => x.assigned_user_id,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_workflow_step_workflow_instance_workflow_instance_id",
                        column: x => x.workflow_instance_id,
                        principalTable: "workflow_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_status_event_workflow_step_id",
                table: "status_event",
                column: "workflow_step_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_instance_company_id",
                table: "workflow_instance",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_step_acted_by",
                table: "workflow_step",
                column: "acted_by");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_step_assigned_user_id",
                table: "workflow_step",
                column: "assigned_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_step_workflow_instance_id",
                table: "workflow_step",
                column: "workflow_instance_id");

            migrationBuilder.AddForeignKey(
                name: "fk_status_event_workflow_steps_workflow_step_id",
                table: "status_event",
                column: "workflow_step_id",
                principalTable: "workflow_step",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_status_event_workflow_steps_workflow_step_id",
                table: "status_event");

            migrationBuilder.DropTable(
                name: "workflow_step");

            migrationBuilder.DropTable(
                name: "workflow_instance");

            migrationBuilder.DropIndex(
                name: "ix_status_event_workflow_step_id",
                table: "status_event");

            migrationBuilder.DropColumn(
                name: "workflow_step_id",
                table: "status_event");
        }
    }
}
