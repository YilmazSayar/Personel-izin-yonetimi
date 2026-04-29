using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personelizin_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRoomsUseUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE ""Permissions"" ADD COLUMN IF NOT EXISTS ""UnitId"" integer;");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Permissions_UnitId"" ON ""Permissions"" (""UnitId"");");
            migrationBuilder.Sql(@"ALTER TABLE ""Permissions"" DROP CONSTRAINT IF EXISTS ""FK_Permissions_Room_RoomId"";");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Permissions_RoomId"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Permissions"" DROP COLUMN IF EXISTS ""RoomId"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""RoomMembers"";");
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""Room"";");
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Permissions_Units_UnitId') THEN
                        ALTER TABLE ""Permissions"" ADD CONSTRAINT ""FK_Permissions_Units_UnitId""
                            FOREIGN KEY (""UnitId"") REFERENCES ""Units""(""Id"");
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Units_UnitId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_UnitId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Permissions");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Permissions",
                type: "integer",
                nullable: true);
        }
    }
}
