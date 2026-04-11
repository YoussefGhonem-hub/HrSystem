using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanIdToPayslipDeduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoanId",
                schema: "Payroll",
                table: "PayslipDeductions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDeductions_LoanId",
                schema: "Payroll",
                table: "PayslipDeductions",
                column: "LoanId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipDeductions_Loans_LoanId",
                schema: "Payroll",
                table: "PayslipDeductions",
                column: "LoanId",
                principalSchema: "Payroll",
                principalTable: "Loans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipDeductions_Loans_LoanId",
                schema: "Payroll",
                table: "PayslipDeductions");

            migrationBuilder.DropIndex(
                name: "IX_PayslipDeductions_LoanId",
                schema: "Payroll",
                table: "PayslipDeductions");

            migrationBuilder.DropColumn(
                name: "LoanId",
                schema: "Payroll",
                table: "PayslipDeductions");
        }
    }
}
