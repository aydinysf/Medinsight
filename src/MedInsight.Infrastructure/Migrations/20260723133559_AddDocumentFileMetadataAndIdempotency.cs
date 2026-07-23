using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedInsight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFileMetadataAndIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_hash",
                table: "medical_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "medical_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "medical_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "size_bytes",
                table: "medical_documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "storage_key",
                table: "medical_documents",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    response_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotency_records", x => x.key);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_records");

            migrationBuilder.DropColumn(
                name: "content_hash",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "size_bytes",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "storage_key",
                table: "medical_documents");
        }
    }
}
