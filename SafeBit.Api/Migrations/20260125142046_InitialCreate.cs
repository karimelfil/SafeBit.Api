using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "safebit");

            migrationBuilder.CreateTable(
                name: "Allergies",
                schema: "safebit",
                columns: table => new
                {
                    AllergyID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.AllergyID);
                });

            migrationBuilder.CreateTable(
                name: "Diseases",
                schema: "safebit",
                columns: table => new
                {
                    DiseaseID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diseases", x => x.DiseaseID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "safebit",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "UserAllergies",
                schema: "safebit",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    AllergyID = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAllergies", x => new { x.UserID, x.AllergyID });
                });

            migrationBuilder.CreateTable(
                name: "UserDiseases",
                schema: "safebit",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    DiseaseID = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDiseases", x => new { x.UserID, x.DiseaseID });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "safebit",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleID = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    IsPregnant = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.InsertData(
                schema: "safebit",
                table: "Allergies",
                columns: new[] { "AllergyID", "Category", "CreatedAt", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Food", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8350), false, "Peanuts", null, null },
                    { 2, "Dairy", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8352), false, "Milk", null, null },
                    { 3, "Food", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8353), false, "Eggs", null, null },
                    { 4, "Food", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8354), false, "Seafood", null, null }
                });

            migrationBuilder.InsertData(
                schema: "safebit",
                table: "Diseases",
                columns: new[] { "DiseaseID", "Category", "CreatedAt", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "Chronic", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8365), false, "Diabetes", null, null },
                    { 2, "Digestive", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8366), false, "Celiac Disease", null, null },
                    { 3, "Chronic", new DateTime(2026, 1, 25, 14, 20, 45, 915, DateTimeKind.Utc).AddTicks(8367), false, "Hypertension", null, null }
                });

            migrationBuilder.InsertData(
                schema: "safebit",
                table: "Roles",
                columns: new[] { "RoleID", "Type" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allergies",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "Diseases",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "UserAllergies",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "UserDiseases",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "safebit");
        }
    }
}
