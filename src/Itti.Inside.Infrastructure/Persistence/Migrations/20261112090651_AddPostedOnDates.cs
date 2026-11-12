using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itti.Inside.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPostedOnDates : Migration
    {
        // Rows that already exist count as posted on the day of the upgrade, so the landing page
        // looks the same straight after it. The admin can change any date on the master page.
        private const string UpgradeDay = "CAST(GETDATE() AS date)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "PostedOn",
                table: "WeddingAnnouncements",
                type: "date",
                nullable: false,
                defaultValueSql: UpgradeDay);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PostedOn",
                table: "NewBornAnnouncements",
                type: "date",
                nullable: false,
                defaultValueSql: UpgradeDay);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PostedOn",
                table: "Birthdays",
                type: "date",
                nullable: false,
                defaultValueSql: UpgradeDay);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostedOn",
                table: "WeddingAnnouncements");

            migrationBuilder.DropColumn(
                name: "PostedOn",
                table: "NewBornAnnouncements");

            migrationBuilder.DropColumn(
                name: "PostedOn",
                table: "Birthdays");
        }
    }
}
