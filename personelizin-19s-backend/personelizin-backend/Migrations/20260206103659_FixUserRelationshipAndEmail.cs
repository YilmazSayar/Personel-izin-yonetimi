using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personelizin_backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelationshipAndEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Email",
                value: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Email",
                value: "");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequests_UserId",
                table: "PermissionRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRequests_Users_UserId",
                table: "PermissionRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRequests_Users_UserId",
                table: "PermissionRequests");

            migrationBuilder.DropIndex(
                name: "IX_PermissionRequests_UserId",
                table: "PermissionRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");
        }
    }
}
