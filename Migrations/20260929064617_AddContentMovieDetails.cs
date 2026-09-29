using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanHubPlus.Migrations
{
    /// <inheritdoc />
    public partial class AddContentMovieDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MerchandiseItems_CategoryId",
                table: "MerchandiseItems");

            migrationBuilder.DropIndex(
                name: "IX_Contents_CategoryId",
                table: "Contents");

            migrationBuilder.AddColumn<string>(
                name: "Review",
                table: "Ratings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeRating",
                table: "Contents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cast",
                table: "Contents",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Contents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionCountry",
                table: "Contents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RuntimeMinutes",
                table: "Contents",
                type: "int",
                nullable: true);

            // ------------------------------------------------------------------
            //  The model stores its converted enums as nvarchar(30) so they can
            //  carry an index (see ApplicationDbContext.OnModelCreating). The
            //  database still has the original nvarchar(max) columns, and SQL
            //  Server refuses to build an index on nvarchar(max) ("Column 'Tag'
            //  ... is of a type that is invalid for use as a key column in an
            //  index"). Narrow the three indexed enum columns first; the index
            //  statements further down then succeed. Every stored value is an
            //  enum name, so nothing truncates.
            // ------------------------------------------------------------------
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Contents",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                table: "MerchandiseItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FanSubmissions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_MerchandiseItems_CategoryId_Tag",
                table: "MerchandiseItems",
                columns: new[] { "CategoryId", "Tag" });

            migrationBuilder.CreateIndex(
                name: "IX_FanSubmissions_CreatedAt",
                table: "FanSubmissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FanSubmissions_Status",
                table: "FanSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Events_City",
                table: "Events",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CategoryId_Type",
                table: "Contents",
                columns: new[] { "CategoryId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Contents_PopularityScore_ViewCount",
                table: "Contents",
                columns: new[] { "PopularityScore", "ViewCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Contents_Type",
                table: "Contents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterProfiles_Fandom",
                table: "CharacterProfiles",
                column: "Fandom");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterProfiles_Name",
                table: "CharacterProfiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsTimeline_CategoryId",
                table: "Articles",
                columns: new[] { "IsTimeline", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishedAt",
                table: "Articles",
                column: "PublishedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MerchandiseItems_CategoryId_Tag",
                table: "MerchandiseItems");

            migrationBuilder.DropIndex(
                name: "IX_FanSubmissions_CreatedAt",
                table: "FanSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_FanSubmissions_Status",
                table: "FanSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_Events_City",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Contents_CategoryId_Type",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_PopularityScore_ViewCount",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_Type",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_CharacterProfiles_Fandom",
                table: "CharacterProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CharacterProfiles_Name",
                table: "CharacterProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_IsTimeline_CategoryId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_PublishedAt",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "Review",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "AgeRating",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Cast",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Director",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "ProductionCountry",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "RuntimeMinutes",
                table: "Contents");

            // Put the converted enum columns back the way the old schema had them.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FanSubmissions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                table: "MerchandiseItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Contents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_MerchandiseItems_CategoryId",
                table: "MerchandiseItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CategoryId",
                table: "Contents",
                column: "CategoryId");
        }
    }
}
