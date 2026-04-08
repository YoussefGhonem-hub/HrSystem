using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPayrollCycleUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollCycles_Month_Year",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_TenantId_Month_Year",
                schema: "Payroll",
                table: "PayrollCycles",
                columns: new[] { "TenantId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollCycles_TenantId_Month_Year",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_Month_Year",
                schema: "Payroll",
                table: "PayrollCycles",
                columns: new[] { "Month", "Year" },
                unique: true);
        }
    }
}
