using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanHubPlus.Migrations
{
    /// <summary>
    /// Brings the database back in sync with the model after the entertainment /
    /// gaming / music / streaming feature (commit a4d3158) was added WITHOUT a
    /// migration. That left several pages throwing HTTP 500:
    ///   "Invalid column name 'AlbumName'" / "'Artist'" /
    ///   "'OfficialWebsiteUrl'" / "'PlayableGameUrl'",
    /// and every query against Playlists failed because the tables did not exist.
    ///
    /// This migration is deliberately ADDITIVE ONLY - it creates new columns and
    /// new tables. It never drops or rewrites an existing column, so applying it
    /// cannot lose data.
    ///
    /// It also emits NO AlterColumn calls for the ASP.NET Core Identity keys.
    /// The model used to declare them as nvarchar(128) (a leftover from the
    /// project's MySQL days) while InitialCreate had actually created them as
    /// nvarchar(450), so the model had silently drifted from the schema and
    /// SQL Server refused any new foreign key with:
    ///   "Column 'AspNetUsers.Id' is not the same length or scale as referencing
    ///    column 'Playlists.UserId' in foreign key ..."
    /// The model in ApplicationDbContext.OnModelCreating now uses the widths the
    /// database really has (450 for keys, 256 for names). That is a model-only
    /// correction - the live columns were already the right size.
    /// </summary>
    public partial class AddContentMediaColumnsAndPlaylists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- Content: media / music / game metadata used by the new pages ----
            migrationBuilder.AddColumn<string>(
                name: "AlbumName",
                table: "Contents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "Contents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialWebsiteUrl",
                table: "Contents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayableGameUrl",
                table: "Contents",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // ---- Playlists: a user's own track lists (/Music/Playlists) ----
            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.PlaylistId);
                    table.ForeignKey(
                        name: "FK_Playlists_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistItems",
                columns: table => new
                {
                    PlaylistItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaylistId = table.Column<int>(type: "int", nullable: false),
                    ContentId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistItems", x => x.PlaylistItemId);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "Contents",
                        principalColumn: "ContentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_ContentId",
                table: "PlaylistItems",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_PlaylistId",
                table: "PlaylistItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistItems");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropColumn(
                name: "AlbumName",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Artist",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "OfficialWebsiteUrl",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "PlayableGameUrl",
                table: "Contents");
        }
    }
}