using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisAndHealthRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "review_priority",
                table: "cases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ai_analyses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    model_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prompt_version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    patient_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_analyses", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_analyses_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_route_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    next_step = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    risk_level = table.Column<int>(type: "integer", nullable: false),
                    triggered_by = table.Column<int>(type: "integer", nullable: false),
                    trigger_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_route_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_health_route_snapshots_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_status = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    next_step = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    risk_level = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_health_routes", x => x.id);
                    table.ForeignKey(
                        name: "fk_health_routes_cases_case_id",
                        column: x => x.case_id,
                        principalTable: "cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_findings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analysis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false),
                    source_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    disclaimer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_findings", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_findings_ai_analyses_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "ai_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "differential_diagnoses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    analysis_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    risk_level = table.Column<int>(type: "integer", nullable: false),
                    source_finding_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_differential_diagnoses", x => x.id);
                    table.ForeignKey(
                        name: "fk_differential_diagnoses_ai_analyses_analysis_id",
                        column: x => x.analysis_id,
                        principalTable: "ai_analyses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_analyses_case_id",
                table: "ai_analyses",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_findings_analysis_id",
                table: "ai_findings",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_findings_case_id",
                table: "ai_findings",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_differential_diagnoses_analysis_id",
                table: "differential_diagnoses",
                column: "analysis_id");

            migrationBuilder.CreateIndex(
                name: "ix_differential_diagnoses_case_id",
                table: "differential_diagnoses",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "ix_health_route_snapshots_case_id_created_at_utc",
                table: "health_route_snapshots",
                columns: new[] { "case_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_health_routes_case_id",
                table: "health_routes",
                column: "case_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_findings");

            migrationBuilder.DropTable(
                name: "differential_diagnoses");

            migrationBuilder.DropTable(
                name: "health_route_snapshots");

            migrationBuilder.DropTable(
                name: "health_routes");

            migrationBuilder.DropTable(
                name: "ai_analyses");

            migrationBuilder.DropColumn(
                name: "review_priority",
                table: "cases");
        }
    }
}
