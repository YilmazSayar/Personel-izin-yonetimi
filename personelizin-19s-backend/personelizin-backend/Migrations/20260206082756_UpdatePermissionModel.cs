using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personelizin_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermissionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentApproverId",
                table: "PermissionRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PermissionRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentApproverId",
                table: "PermissionRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PermissionRequests");
        }
    }
}
