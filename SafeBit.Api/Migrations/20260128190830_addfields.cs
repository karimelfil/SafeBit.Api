using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class addfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "safebit",
                table: "Diseases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "safebit",
                table: "Allergies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9625), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9627), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9628), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9629), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9642), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9643), null });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                columns: new[] { "CreatedAt", "DeletedAt" },
                values: new object[] { new DateTime(2026, 1, 28, 19, 8, 29, 993, DateTimeKind.Utc).AddTicks(9644), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "safebit",
                table: "Diseases");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "safebit",
                table: "Allergies");

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5790));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5792));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5793));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5794));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5805));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5806));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 20, 2, 56, 133, DateTimeKind.Utc).AddTicks(5807));
        }
    }
}
