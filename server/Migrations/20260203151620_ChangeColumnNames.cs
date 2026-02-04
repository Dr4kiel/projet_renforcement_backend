using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Tags",
                newName: "tag_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "of_",
                table: "ofs",
                newName: "of_name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ofs",
                newName: "of_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Lines",
                newName: "line_id");

            migrationBuilder.RenameColumn(
                name: "value_",
                table: "Historian",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "timestamp_",
                table: "Historian",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Historian",
                newName: "historian_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Equipments",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Equipments",
                newName: "equipment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tag_id",
                table: "Tags",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Roles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "of_name",
                table: "ofs",
                newName: "of_");

            migrationBuilder.RenameColumn(
                name: "of_id",
                table: "ofs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "line_id",
                table: "Lines",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "Historian",
                newName: "value_");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "Historian",
                newName: "timestamp_");

            migrationBuilder.RenameColumn(
                name: "historian_id",
                table: "Historian",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Equipments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "equipment_id",
                table: "Equipments",
                newName: "Id");
        }
    }
}
