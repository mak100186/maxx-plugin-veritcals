using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maxx.PluginVerticals.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddedStars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stars",
                table: "Articles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stars",
                table: "Articles");
        }
    }
}
