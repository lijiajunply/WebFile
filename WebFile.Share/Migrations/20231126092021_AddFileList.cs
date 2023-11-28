using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebFile.BlazorServer.Migrations
{
    /// <inheritdoc />
    public partial class AddFileList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileModel",
                columns: table => new
                {
                    Path = table.Column<string>(type: "varchar(256)", nullable: false),
                    UserModelUserName = table.Column<string>(type: "varchar(256)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileModel", x => x.Path);
                    table.ForeignKey(
                        name: "FK_FileModel_Users_UserModelUserName",
                        column: x => x.UserModelUserName,
                        principalTable: "Users",
                        principalColumn: "UserName");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileModel_UserModelUserName",
                table: "FileModel",
                column: "UserModelUserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileModel");
        }
    }
}
