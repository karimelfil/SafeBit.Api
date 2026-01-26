using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoutSupportl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveJti",
                schema: "safebit",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogoutAt",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveJti",
                schema: "safebit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLogoutAt",
                schema: "safebit",
                table: "Users");

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
    }
}
