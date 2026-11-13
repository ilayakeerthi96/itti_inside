using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itti.Inside.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AwardPictureGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RockstarAwardImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AwardId = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RockstarAwardImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RockstarAwardImages_PortalImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "PortalImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RockstarAwardImages_RockstarAwards_AwardId",
                        column: x => x.AwardId,
                        principalTable: "RockstarAwards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RockstarAwardImages_AwardId_Position",
                table: "RockstarAwardImages",
                columns: new[] { "AwardId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_RockstarAwardImages_ImageId",
                table: "RockstarAwardImages",
                column: "ImageId");

            // Each award keeps the picture it has, as the first in its new list.
            migrationBuilder.Sql("""
                INSERT INTO [RockstarAwardImages] ([AwardId], [ImageId], [Position])
                SELECT [Id], [ImageId], 0 FROM [RockstarAwards] WHERE [ImageId] IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_RockstarAwards_PortalImages_ImageId",
                table: "RockstarAwards");

            migrationBuilder.DropIndex(
                name: "IX_RockstarAwards_ImageId",
                table: "RockstarAwards");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "RockstarAwards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "RockstarAwards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RockstarAwards_ImageId",
                table: "RockstarAwards",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_RockstarAwards_PortalImages_ImageId",
                table: "RockstarAwards",
                column: "ImageId",
                principalTable: "PortalImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Each award keeps its first picture as its one picture.
            migrationBuilder.Sql("""
                UPDATE [award] SET [ImageId] = [link].[ImageId]
                FROM [RockstarAwards] AS [award]
                JOIN [RockstarAwardImages] AS [link] ON [link].[AwardId] = [award].[Id] AND [link].[Position] = 0;
                """);

            migrationBuilder.DropTable(
                name: "RockstarAwardImages");
        }
    }
}
