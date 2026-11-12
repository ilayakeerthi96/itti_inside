using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itti.Inside.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "RockstarAwards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "CeoDeskPosts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PortalImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RockstarAwards_ImageId",
                table: "RockstarAwards",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_CeoDeskPosts_ImageId",
                table: "CeoDeskPosts",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CeoDeskPosts_PortalImages_ImageId",
                table: "CeoDeskPosts",
                column: "ImageId",
                principalTable: "PortalImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RockstarAwards_PortalImages_ImageId",
                table: "RockstarAwards",
                column: "ImageId",
                principalTable: "PortalImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CeoDeskPosts_PortalImages_ImageId",
                table: "CeoDeskPosts");

            migrationBuilder.DropForeignKey(
                name: "FK_RockstarAwards_PortalImages_ImageId",
                table: "RockstarAwards");

            migrationBuilder.DropTable(
                name: "PortalImages");

            migrationBuilder.DropIndex(
                name: "IX_RockstarAwards_ImageId",
                table: "RockstarAwards");

            migrationBuilder.DropIndex(
                name: "IX_CeoDeskPosts_ImageId",
                table: "CeoDeskPosts");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "RockstarAwards");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "CeoDeskPosts");
        }
    }
}
