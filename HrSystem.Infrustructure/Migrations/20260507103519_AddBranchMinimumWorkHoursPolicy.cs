using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchMinimumWorkHoursPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AbsentThresholdHours",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CheckInWindowMinutes",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBreakTimeDeducted",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOvertimeEnabled",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumFullDayHours",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumHalfDayHours",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeStartsAfterHours",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShiftTotalHours",
                schema: "Organization",
                table: "BranchWorkSchedules",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsentThresholdHours",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "CheckInWindowMinutes",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "IsBreakTimeDeducted",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "IsOvertimeEnabled",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "MinimumFullDayHours",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "MinimumHalfDayHours",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "OvertimeStartsAfterHours",
                schema: "Organization",
                table: "BranchWorkSchedules");

            migrationBuilder.DropColumn(
                name: "ShiftTotalHours",
                schema: "Organization",
                table: "BranchWorkSchedules");
        }
    }
}
