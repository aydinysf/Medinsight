using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDicomGroupingAndTextExtraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "extracted_text",
                table: "medical_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ocr_confidence",
                table: "medical_documents",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_grouped",
                table: "dicom_studies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_file_received_at_utc",
                table: "dicom_studies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "modality",
                table: "dicom_series",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "slice_count",
                table: "dicom_series",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "extracted_text",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "ocr_confidence",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "is_grouped",
                table: "dicom_studies");

            migrationBuilder.DropColumn(
                name: "last_file_received_at_utc",
                table: "dicom_studies");

            migrationBuilder.DropColumn(
                name: "modality",
                table: "dicom_series");

            migrationBuilder.DropColumn(
                name: "slice_count",
                table: "dicom_series");
        }
    }
}
