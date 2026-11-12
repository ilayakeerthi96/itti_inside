using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itti.Inside.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHrPagesAndCeoDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CeoDeskPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PostedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CeoDeskPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalInsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Insurer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Administrator = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: false),
                    SumInsured = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Coverage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Benefits = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ClaimSteps = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NetworkHospitalsLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HelpdeskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HelpdeskPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HelpdeskEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalInsurancePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortalDocuments",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalDocuments", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Venue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Trainer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Registration = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date");

            // The single CEO's message becomes the first CEO's Desk post, dated the day of the upgrade.
            migrationBuilder.Sql("""
                INSERT INTO [CeoDeskPosts] ([Title], [PostedOn], [Message])
                SELECT N'A message from the CEO', CAST(GETDATE() AS date), [Value]
                FROM [PortalTexts]
                WHERE [Key] = N'CeoMessage' AND LEN([Value]) > 0;

                DELETE FROM [PortalTexts] WHERE [Key] = N'CeoMessage';
                """);

            // The 2026 holidays, from HR's "ITTI_HOLIDAY LIST - 2026_1.0.docx", for every database.
            migrationBuilder.InsertData(
                table: "Holidays",
                columns: ["Date", "Name", "IsOptional"],
                values: new object[,]
                {
                    { new DateOnly(2026, 1, 1), "New Year", false },
                    { new DateOnly(2026, 1, 14), "Makara Sankranti", false },
                    { new DateOnly(2026, 1, 26), "Republic Day", false },
                    { new DateOnly(2026, 3, 4), "Holi", true },
                    { new DateOnly(2026, 3, 19), "Ugadi / Gudi Padwa", false },
                    { new DateOnly(2026, 4, 3), "Good Friday", false },
                    { new DateOnly(2026, 5, 1), "May Day (International Worker’s Day)", false },
                    { new DateOnly(2026, 5, 27), "Bakrid", true },
                    { new DateOnly(2026, 9, 14), "Ganesha Chaturthi", false },
                    { new DateOnly(2026, 10, 2), "Gandhi Jayanti", false },
                    { new DateOnly(2026, 10, 20), "Mahanavami / Ayudha Pooja", false },
                    { new DateOnly(2026, 10, 21), "Vijaya Dashami / Dussehra", false },
                    { new DateOnly(2026, 11, 10), "Balipadiyami / Diwali", false },
                    { new DateOnly(2026, 12, 25), "Christmas", false },
                });

            // The notes from the same file. A new, empty database gets them with the rest of its first content.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [PortalTexts]) AND NOT EXISTS (SELECT 1 FROM [PortalTexts] WHERE [Key] = N'HolidayNotes')
                    INSERT INTO [PortalTexts] ([Key], [Value])
                    VALUES (N'HolidayNotes',
                        N'Associates directly reports to the Customer place/handle the projects would be following the Customer holiday list.'
                        + NCHAR(10)
                        + N'The List does not Include INDEPENDENCE DAY & KANNADA RAJYOTSAVA which falls on Saturday & Sunday as declared by Karnataka State Government.');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Puts the newest CEO's Desk post back as the single CEO's message.
            migrationBuilder.Sql("""
                INSERT INTO [PortalTexts] ([Key], [Value])
                SELECT TOP (1) N'CeoMessage', [Message] FROM [CeoDeskPosts] ORDER BY [PostedOn] DESC, [Id] DESC;

                DELETE FROM [PortalTexts] WHERE [Key] = N'HolidayNotes';
                """);

            migrationBuilder.DropTable(
                name: "CeoDeskPosts");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "MedicalInsurancePolicies");

            migrationBuilder.DropTable(
                name: "PortalDocuments");

            migrationBuilder.DropTable(
                name: "TrainingSessions");
        }
    }
}
