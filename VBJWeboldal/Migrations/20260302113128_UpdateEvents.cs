using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VBJWeboldal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Events",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Events",
                newName: "Location");
        }
    }
}
