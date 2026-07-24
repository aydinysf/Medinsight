using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationAndTreatment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correction_notes",
                table: "ai_analyses",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "review_decision",
                table: "ai_analyses",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reviewed_at_utc",
                table: "ai_analyses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reviewed_by_doctor_id",
                table: "ai_analyses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "consultations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consultations", x => x.id);
                    table.ForeignKey(
                        name: "fk_consultations_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "treatments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    follow_up_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_treatments", x => x.id);
                    table.ForeignKey(
                        name: "fk_treatments_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clinical_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clinical_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_clinical_notes_consultations_consultation_id",
                        column: x => x.consultation_id,
                        principalTable: "consultations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consultation_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    consultation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consultation_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_consultation_messages_consultations_consultation_id",
                        column: x => x.consultation_id,
                        principalTable: "consultations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clinical_notes_consultation_id",
                table: "clinical_notes",
                column: "consultation_id");

            migrationBuilder.CreateIndex(
                name: "ix_consultation_messages_consultation_id_created_at_utc",
                table: "consultation_messages",
                columns: new[] { "consultation_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_consultations_case_id_status",
                table: "consultations",
                columns: new[] { "case_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_consultations_doctor_id",
                table: "consultations",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "ix_treatments_case_id",
                table: "treatments",
                column: "case_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clinical_notes");

            migrationBuilder.DropTable(
                name: "consultation_messages");

            migrationBuilder.DropTable(
                name: "treatments");

            migrationBuilder.DropTable(
                name: "consultations");

            migrationBuilder.DropColumn(
                name: "correction_notes",
                table: "ai_analyses");

            migrationBuilder.DropColumn(
                name: "review_decision",
                table: "ai_analyses");

            migrationBuilder.DropColumn(
                name: "reviewed_at_utc",
                table: "ai_analyses");

            migrationBuilder.DropColumn(
                name: "reviewed_by_doctor_id",
                table: "ai_analyses");
        }
    }
}
