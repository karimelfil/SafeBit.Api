using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SafeBit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuScanDishIngredientFeedbackTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ingredients",
                schema: "safebit",
                columns: table => new
                {
                    IngredientID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DishName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.IngredientID);
                });

            migrationBuilder.CreateTable(
                name: "MenuUploads",
                schema: "safebit",
                columns: table => new
                {
                    MenuID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    RestaurantName = table.Column<string>(type: "text", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuUploads", x => x.MenuID);
                    table.ForeignKey(
                        name: "FK_MenuUploads_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "safebit",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dishes",
                schema: "safebit",
                columns: table => new
                {
                    DishID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuID = table.Column<int>(type: "integer", nullable: false),
                    DishName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsSafe = table.Column<bool>(type: "boolean", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.DishID);
                    table.ForeignKey(
                        name: "FK_Dishes_MenuUploads_MenuID",
                        column: x => x.MenuID,
                        principalSchema: "safebit",
                        principalTable: "MenuUploads",
                        principalColumn: "MenuID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScanHistories",
                schema: "safebit",
                columns: table => new
                {
                    ScanID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    MenuID = table.Column<int>(type: "integer", nullable: false),
                    ScanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResultsSummary = table.Column<string>(type: "text", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanHistories", x => x.ScanID);
                    table.ForeignKey(
                        name: "FK_ScanHistories_MenuUploads_MenuID",
                        column: x => x.MenuID,
                        principalSchema: "safebit",
                        principalTable: "MenuUploads",
                        principalColumn: "MenuID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScanHistories_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "safebit",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DishIngredients",
                schema: "safebit",
                columns: table => new
                {
                    DishID = table.Column<int>(type: "integer", nullable: false),
                    IngredientID = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishIngredients", x => new { x.DishID, x.IngredientID });
                    table.ForeignKey(
                        name: "FK_DishIngredients_Dishes_DishID",
                        column: x => x.DishID,
                        principalSchema: "safebit",
                        principalTable: "Dishes",
                        principalColumn: "DishID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishIngredients_Ingredients_IngredientID",
                        column: x => x.IngredientID,
                        principalSchema: "safebit",
                        principalTable: "Ingredients",
                        principalColumn: "IngredientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackReports",
                schema: "safebit",
                columns: table => new
                {
                    ReportID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    DishID = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackReports", x => x.ReportID);
                    table.ForeignKey(
                        name: "FK_FeedbackReports_Dishes_DishID",
                        column: x => x.DishID,
                        principalSchema: "safebit",
                        principalTable: "Dishes",
                        principalColumn: "DishID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeedbackReports_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "safebit",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(902));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(904));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(905));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(906));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(921));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(922));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 19, 25, 55, 205, DateTimeKind.Utc).AddTicks(923));

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_MenuID",
                schema: "safebit",
                table: "Dishes",
                column: "MenuID");

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_IngredientID",
                schema: "safebit",
                table: "DishIngredients",
                column: "IngredientID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReports_DishID",
                schema: "safebit",
                table: "FeedbackReports",
                column: "DishID");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReports_UserID",
                schema: "safebit",
                table: "FeedbackReports",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuUploads_UserID",
                schema: "safebit",
                table: "MenuUploads",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_MenuID",
                schema: "safebit",
                table: "ScanHistories",
                column: "MenuID");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistories_UserID",
                schema: "safebit",
                table: "ScanHistories",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishIngredients",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "FeedbackReports",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "ScanHistories",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "Ingredients",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "Dishes",
                schema: "safebit");

            migrationBuilder.DropTable(
                name: "MenuUploads",
                schema: "safebit");

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(743));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(746));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(747));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Allergies",
                keyColumn: "AllergyID",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(749));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(765));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(767));

            migrationBuilder.UpdateData(
                schema: "safebit",
                table: "Diseases",
                keyColumn: "DiseaseID",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 29, 14, 35, 16, 766, DateTimeKind.Utc).AddTicks(768));
        }
    }
}
