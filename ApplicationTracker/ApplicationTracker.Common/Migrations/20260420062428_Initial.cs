using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApplicationTracker.Common.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedinConnected = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedinAccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedinRefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedinTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LinkedinId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XingConnected = table.Column<bool>(type: "boolean", nullable: false),
                    XingAccessToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XingRefreshToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XingTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    XingId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Company = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Position = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "submitted"),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JobUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompanyWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Contact = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Salary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LinkedinProfile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XingProfile = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LinkedinProfileLinked = table.Column<bool>(type: "boolean", nullable: false),
                    XingProfileLinked = table.Column<bool>(type: "boolean", nullable: false),
                    LinkedinData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    XingData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationNotes_Applications",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationReminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReminderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationReminders_Applications",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_ApplicationId",
                table: "ApplicationNotes",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationNotes_CreatedAt",
                table: "ApplicationNotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationReminders_ApplicationId",
                table: "ApplicationReminders",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationReminders_IsCompleted",
                table: "ApplicationReminders",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationReminders_ReminderDate",
                table: "ApplicationReminders",
                column: "ReminderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Company",
                table: "Applications",
                column: "Company");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_InterviewDate",
                table: "Applications",
                column: "InterviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status",
                table: "Applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_SubmittedDate",
                table: "Applications",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_UserId_Status",
                table: "Applications",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Unique",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationNotes");

            migrationBuilder.DropTable(
                name: "ApplicationReminders");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
