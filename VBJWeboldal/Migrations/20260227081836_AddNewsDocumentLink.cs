using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VBJWeboldal.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsDocumentLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachedDocumentUrl",
                table: "News",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachedDocumentUrl",
                table: "News");
        }
    }
}
