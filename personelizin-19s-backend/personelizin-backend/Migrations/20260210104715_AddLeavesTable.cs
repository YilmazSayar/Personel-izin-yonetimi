using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace personelizin_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLeavesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRequests_Users_UserId",
                table: "PermissionRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionRequests",
                table: "PermissionRequests");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameTable(
                name: "PermissionRequests",
                newName: "PermissionRequest");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRequests_UserId",
                table: "PermissionRequest",
                newName: "IX_PermissionRequest_UserId");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionRequest",
                table: "PermissionRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRequest_Users_UserId",
                table: "PermissionRequest",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissionRequest_Users_UserId",
                table: "PermissionRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PermissionRequest",
                table: "PermissionRequest");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "PermissionRequest",
                newName: "PermissionRequests");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_PermissionRequest_UserId",
                table: "PermissionRequests",
                newName: "IX_PermissionRequests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PermissionRequests",
                table: "PermissionRequests",
                column: "Id");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Role" },
                values: new object[,]
                {
                    { 1, "", "Bülent D.", "Manager" },
                    { 2, "", "Bülent Ç.", "Director" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PermissionRequests_Users_UserId",
                table: "PermissionRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
