using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAllowanceDeductionTypeIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayslipAllowances_AllowanceTypes_AllowanceTypeId",
                table: "PayslipAllowances");

            migrationBuilder.DropForeignKey(
                name: "FK_PayslipDeductions_DeductionTypes_DeductionTypeId",
                table: "PayslipDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryAllowances_AllowanceTypes_AllowanceTypeId",
                table: "SalaryAllowances");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryDeductions_DeductionTypes_DeductionTypeId",
                table: "SalaryDeductions");

            migrationBuilder.DropIndex(
                name: "IX_SalaryDeductions_DeductionTypeId",
                table: "SalaryDeductions");

            migrationBuilder.DropIndex(
                name: "IX_SalaryAllowances_AllowanceTypeId",
                table: "SalaryAllowances");

            migrationBuilder.DropIndex(
                name: "IX_PayslipDeductions_DeductionTypeId",
                table: "PayslipDeductions");

            migrationBuilder.DropIndex(
                name: "IX_PayslipAllowances_AllowanceTypeId",
                table: "PayslipAllowances");

            migrationBuilder.DropColumn(
                name: "DeductionTypeId",
                table: "SalaryDeductions");

            migrationBuilder.DropColumn(
                name: "AllowanceTypeId",
                table: "SalaryAllowances");

            migrationBuilder.DropColumn(
                name: "DeductionTypeId",
                table: "PayslipDeductions");

            migrationBuilder.DropColumn(
                name: "AllowanceTypeId",
                table: "PayslipAllowances");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "SalaryDeductions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubjectToInsurance",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxable",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SalaryDeductions");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "SalaryDeductions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "SalaryDeductions");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "SalaryDeductions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SalaryAllowances");

            migrationBuilder.DropColumn(
                name: "IsSubjectToInsurance",
                table: "SalaryAllowances");

            migrationBuilder.DropColumn(
                name: "IsTaxable",
                table: "SalaryAllowances");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "SalaryAllowances");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "SalaryAllowances");

            migrationBuilder.AddColumn<Guid>(
                name: "DeductionTypeId",
                table: "SalaryDeductions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AllowanceTypeId",
                table: "SalaryAllowances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeductionTypeId",
                table: "PayslipDeductions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AllowanceTypeId",
                table: "PayslipAllowances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_DeductionTypeId",
                table: "SalaryDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAllowances_AllowanceTypeId",
                table: "SalaryAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDeductions_DeductionTypeId",
                table: "PayslipDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipAllowances_AllowanceTypeId",
                table: "PayslipAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipAllowances_AllowanceTypes_AllowanceTypeId",
                table: "PayslipAllowances",
                column: "AllowanceTypeId",
                principalSchema: "Payroll",
                principalTable: "AllowanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayslipDeductions_DeductionTypes_DeductionTypeId",
                table: "PayslipDeductions",
                column: "DeductionTypeId",
                principalSchema: "Payroll",
                principalTable: "DeductionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryAllowances_AllowanceTypes_AllowanceTypeId",
                table: "SalaryAllowances",
                column: "AllowanceTypeId",
                principalSchema: "Payroll",
                principalTable: "AllowanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryDeductions_DeductionTypes_DeductionTypeId",
                table: "SalaryDeductions",
                column: "DeductionTypeId",
                principalSchema: "Payroll",
                principalTable: "DeductionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
