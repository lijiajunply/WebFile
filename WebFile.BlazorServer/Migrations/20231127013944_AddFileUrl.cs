using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "FileModel",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "FileModel");
        }
    }
}
