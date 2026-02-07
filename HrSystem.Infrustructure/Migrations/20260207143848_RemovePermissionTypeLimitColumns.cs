using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePermissionTypeLimitColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeductsFromLeave",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "HoursPerLeaveDay",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "MaxHoursPerMonth",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "MaxHoursPerRequest",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "PermissionTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeductsFromLeave",
                schema: "Requests",
                table: "PermissionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HoursPerLeaveDay",
                schema: "Requests",
                table: "PermissionTypes",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxHoursPerMonth",
                schema: "Requests",
                table: "PermissionTypes",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxHoursPerRequest",
                schema: "Requests",
                table: "PermissionTypes",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "PermissionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
