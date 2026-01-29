using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_PasswordReset_Token : Migration
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
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4115));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4118));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4119));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4121));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4142));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4145));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 3, 18, 71, DateTimeKind.Utc).AddTicks(4146));
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
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6842));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6844));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6844));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6845));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6857));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6858));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 25, 16, 12, 11, 796, DateTimeKind.Utc).AddTicks(6859));
        }
    }
}
