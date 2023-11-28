using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFolder",
                table: "FileModel",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFolder",
                table: "FileModel");
        }
    }
}
