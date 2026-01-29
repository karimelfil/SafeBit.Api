using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFieldsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "safebit",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                schema: "safebit",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "safebit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                schema: "safebit",
                table: "Users");

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
    }
}
