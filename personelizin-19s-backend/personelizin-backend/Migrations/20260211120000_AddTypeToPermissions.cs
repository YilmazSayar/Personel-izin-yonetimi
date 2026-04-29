using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personelizin_backend.Migrations
{
    public partial class AddTypeToPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Permissions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Permissions");
        }
    }
}
