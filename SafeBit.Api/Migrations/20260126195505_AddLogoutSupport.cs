using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoutSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6981));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6984));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6985));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6986));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6998));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(6999));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 55, 5, 552, DateTimeKind.Utc).AddTicks(7000));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(7986));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(7988));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(7989));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(7990));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(8001));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(8002));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 19, 58, 17, DateTimeKind.Utc).AddTicks(8003));
        }
    }
}
