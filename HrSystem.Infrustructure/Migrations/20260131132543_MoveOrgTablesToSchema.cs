using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveOrgTablesToSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "BranchWorkSchedules",
                newName: "BranchWorkSchedules",
                newSchema: "Organization");

            migrationBuilder.RenameTable(
                name: "BranchHolidays",
                newName: "BranchHolidays",
                newSchema: "Organization");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Organization",
                table: "BranchHolidays",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Organization",
                table: "BranchHolidays",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Organization",
                table: "BranchHolidays",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_BranchId_Date_Year",
                schema: "Organization",
                table: "BranchHolidays",
                columns: new[] { "BranchId", "Date", "Year" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchHolidays_BranchId_Date_Year",
                schema: "Organization",
                table: "BranchHolidays");

            migrationBuilder.RenameTable(
                name: "BranchWorkSchedules",
                schema: "Organization",
                newName: "BranchWorkSchedules");

            migrationBuilder.RenameTable(
                name: "BranchHolidays",
                schema: "Organization",
                newName: "BranchHolidays");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "BranchWorkSchedules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "BranchWorkSchedules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "BranchHolidays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "BranchHolidays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BranchHolidays",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
