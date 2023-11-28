using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFileId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "FileModel",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(256)");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "FileModel",
                type: "varchar(256)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FileModel");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "FileModel",
                type: "varchar(256)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileModel",
                table: "FileModel",
                column: "Path");
        }
    }
}
