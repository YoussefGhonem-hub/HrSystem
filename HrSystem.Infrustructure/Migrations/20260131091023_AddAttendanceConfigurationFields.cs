using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceConfigurationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeId_Date",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.AddColumn<string>(
                name: "AbsenceDeductionPolicy",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttendanceMethod",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GracePeriod",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HalfDayRule",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfigurationRecord",
                schema: "Attendance",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LateDeductionPolicy",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaxLatePerMonth",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MissingCheckoutHandling",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OvertimeCalculation",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OvertimeEligible",
                schema: "Attendance",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WorkDays",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkShift",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_Date_IsConfigurationRecord",
                schema: "Attendance",
                table: "Attendances",
                columns: new[] { "EmployeeId", "Date", "IsConfigurationRecord" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attendances_EmployeeId_Date_IsConfigurationRecord",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AbsenceDeductionPolicy",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "AttendanceMethod",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "GracePeriod",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "HalfDayRule",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsConfigurationRecord",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "LateDeductionPolicy",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "MaxLatePerMonth",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "MissingCheckoutHandling",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "OvertimeCalculation",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "OvertimeEligible",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "WorkDays",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "WorkShift",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_Date",
                schema: "Attendance",
                table: "Attendances",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);
        }
    }
}
