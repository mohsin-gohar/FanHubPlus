using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanHubPlus.Migrations
{
    /// <summary>
    /// Adds the optional fan-art image URL introduced after the initial schema.
    /// The identity key-length changes that also appeared in the original
    /// MySQL-oriented migration are intentionally not replayed here: SQL
    /// Server accepts the wider original columns and existing FKs depend on
    /// them, so shrinking those columns would require rebuilding constraints.
    /// </summary>
    public partial class AddFanSubmissionImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "FanSubmissions",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "FanSubmissions");
        }
    }
}
