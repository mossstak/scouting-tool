using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScoutingTool.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerTeamsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClubTeamName",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternationalTeamName",
                table: "Players",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClubTeamName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "InternationalTeamName",
                table: "Players");
        }
    }
}
