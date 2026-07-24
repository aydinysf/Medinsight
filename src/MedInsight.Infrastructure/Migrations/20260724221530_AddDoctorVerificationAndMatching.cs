using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorVerificationAndMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "active_case_count",
                table: "doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "capacity_threshold",
                table: "doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "manual_override",
                table: "doctors",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "override_expires_at",
                table: "doctors",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "years_of_experience",
                table: "doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "doctor_verifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<int>(type: "integer", nullable: false),
                    document_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    qr_payload = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    qr_parsed_data = table.Column<string>(type: "jsonb", nullable: true),
                    method = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    verified_by_admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_doctor_verifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_doctor_verifications_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reviewer_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    specialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    years_of_experience = table.Column<int>(type: "integer", nullable: false),
                    case_review_count = table.Column<int>(type: "integer", nullable: false),
                    average_response_time_minutes = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    correction_rate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    correction_count = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reviewer_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviewer_profiles_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_doctor_verifications_doctor_id",
                table: "doctor_verifications",
                column: "doctor_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviewer_profiles_doctor_id",
                table: "reviewer_profiles",
                column: "doctor_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "doctor_verifications");

            migrationBuilder.DropTable(
                name: "reviewer_profiles");

            migrationBuilder.DropColumn(
                name: "active_case_count",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "capacity_threshold",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "manual_override",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "override_expires_at",
                table: "doctors");

            migrationBuilder.DropColumn(
                name: "years_of_experience",
                table: "doctors");
        }
    }
}
