using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itti.Inside.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSampleSlideWording : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drops "Sample slide." from the start of the starting hero slides and keeps the rest of the
            // wording, for example "News, people and applications for every ITTIan." Slides written on the
            // master page don't start with it, so they stay as they are.
            migrationBuilder.Sql("""
                UPDATE [HeroSlides]
                SET [Subtitle] = LTRIM(SUBSTRING([Subtitle], LEN(N'Sample slide.') + 1, 4000))
                WHERE [Subtitle] LIKE N'Sample slide.%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: the removed words were sample text.
        }
    }
}
