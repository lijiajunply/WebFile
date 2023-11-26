using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFileToUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileModel_Users_UserModelUserName",
                table: "FileModel");

            migrationBuilder.DropIndex(
                name: "IX_FileModel_UserModelUserName",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "UserModelUserName",
                table: "FileModel");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserName",
                table: "FileModel",
                type: "varchar(256)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FileModel_OwnerUserName",
                table: "FileModel",
                column: "OwnerUserName");

            migrationBuilder.AddForeignKey(
                name: "FK_FileModel_Users_OwnerUserName",
                table: "FileModel",
                column: "OwnerUserName",
                principalTable: "Users",
                principalColumn: "UserName",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileModel_Users_OwnerUserName",
                table: "FileModel");

            migrationBuilder.DropIndex(
                name: "IX_FileModel_OwnerUserName",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "OwnerUserName",
                table: "FileModel");

            migrationBuilder.AddColumn<string>(
                name: "UserModelUserName",
                table: "FileModel",
                type: "varchar(256)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileModel_UserModelUserName",
                table: "FileModel",
                column: "UserModelUserName");

            migrationBuilder.AddForeignKey(
                name: "FK_FileModel_Users_UserModelUserName",
                table: "FileModel",
                column: "UserModelUserName",
                principalTable: "Users",
                principalColumn: "UserName");
        }
    }
}
