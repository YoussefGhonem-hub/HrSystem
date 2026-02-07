using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureEntitySchemaConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AttendanceStatuses_StatusId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Countries_CountryId1",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatuses_StatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Genders_GenderId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_MaritalStatuses_MaritalStatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId",
                table: "KPIEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CountryId1",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "securety",
                newName: "UserTokens",
                newSchema: "security");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "securety",
                newName: "UserRoles",
                newSchema: "security");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                schema: "securety",
                newName: "UserLogins",
                newSchema: "security");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                schema: "securety",
                newName: "UserClaims",
                newSchema: "security");

            migrationBuilder.RenameTable(
                name: "SalaryDeductions",
                newName: "SalaryDeductions",
                newSchema: "Payroll");

            migrationBuilder.RenameTable(
                name: "SalaryAllowances",
                newName: "SalaryAllowances",
                newSchema: "Payroll");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                schema: "securety",
                newName: "RoleClaims",
                newSchema: "security");

            migrationBuilder.RenameTable(
                name: "PayslipDeductions",
                newName: "PayslipDeductions",
                newSchema: "Payroll");

            migrationBuilder.RenameTable(
                name: "PayslipAllowances",
                newName: "PayslipAllowances",
                newSchema: "Payroll");

            migrationBuilder.RenameTable(
                name: "PayrollStatuses",
                newName: "PayrollStatuses",
                newSchema: "Payroll");

            migrationBuilder.RenameTable(
                name: "OrganizationInvoiceItems",
                newName: "OrganizationInvoiceItems",
                newSchema: "Organization");

            migrationBuilder.RenameTable(
                name: "MaritalStatuses",
                newName: "MaritalStatuses",
                newSchema: "Employee");

            migrationBuilder.RenameTable(
                name: "KPIEvaluations",
                newName: "KPIEvaluations",
                newSchema: "Performance");

            migrationBuilder.RenameTable(
                name: "Goals",
                newName: "Goals",
                newSchema: "Performance");

            migrationBuilder.RenameTable(
                name: "GoalMilestones",
                newName: "GoalMilestones",
                newSchema: "Performance");

            migrationBuilder.RenameTable(
                name: "Genders",
                newName: "Genders",
                newSchema: "Employee");

            migrationBuilder.RenameTable(
                name: "EmployeeStatuses",
                newName: "EmployeeStatuses",
                newSchema: "Employee");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Countries",
                newSchema: "Organization");

            migrationBuilder.RenameTable(
                name: "ContractTypes",
                newName: "ContractTypes",
                newSchema: "Employee");

            migrationBuilder.RenameTable(
                name: "AttendanceStatuses",
                newName: "AttendanceStatuses",
                newSchema: "Attendance");

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageValue",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRecurring",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPercentage",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Payroll",
                table: "SalaryDeductions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageValue",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTaxable",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSubjectToInsurance",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPercentage",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Payroll",
                table: "SalaryAllowances",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeductionNameEn",
                schema: "Payroll",
                table: "PayslipDeductions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeductionNameAr",
                schema: "Payroll",
                table: "PayslipDeductions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AllowanceNameEn",
                schema: "Payroll",
                table: "PayslipAllowances",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AllowanceNameAr",
                schema: "Payroll",
                table: "PayslipAllowances",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                schema: "Payroll",
                table: "PayrollStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                schema: "Organization",
                table: "OrganizationInvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEn",
                schema: "Organization",
                table: "OrganizationInvoiceItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionAr",
                schema: "Organization",
                table: "OrganizationInvoiceItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Employee",
                table: "MaritalStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Employee",
                table: "MaritalStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "MaritalStatuses",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Employee",
                table: "MaritalStatuses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                schema: "Performance",
                table: "KPIEvaluations",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Evidence",
                schema: "Performance",
                table: "KPIEvaluations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                schema: "Performance",
                table: "KPIEvaluations",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KPIId1",
                schema: "Performance",
                table: "KPIEvaluations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                schema: "Performance",
                table: "GoalMilestones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TitleAr",
                schema: "Performance",
                table: "GoalMilestones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "Performance",
                table: "GoalMilestones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                schema: "Performance",
                table: "GoalMilestones",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Employee",
                table: "Genders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Employee",
                table: "Genders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "Genders",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Employee",
                table: "Genders",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                schema: "Employee",
                table: "EmployeeStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneCode",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Organization",
                table: "Countries",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Organization",
                table: "Countries",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Organization",
                table: "Countries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Employee",
                table: "ContractTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Employee",
                table: "ContractTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "ContractTypes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Employee",
                table: "ContractTypes",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Employee",
                table: "ContractTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                schema: "Attendance",
                table: "AttendanceStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollStatuses_TenantId_NameEn",
                schema: "Payroll",
                table: "PayrollStatuses",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaritalStatuses_TenantId_NameEn",
                schema: "Employee",
                table: "MaritalStatuses",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPIEvaluations_KPIId1",
                schema: "Performance",
                table: "KPIEvaluations",
                column: "KPIId1");

            migrationBuilder.CreateIndex(
                name: "IX_Genders_TenantId_NameEn",
                schema: "Employee",
                table: "Genders",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeStatuses_TenantId_NameEn",
                schema: "Employee",
                table: "EmployeeStatuses",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                schema: "Organization",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_TenantId_NameEn",
                schema: "Employee",
                table: "ContractTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceStatuses_TenantId_NameEn",
                schema: "Attendance",
                table: "AttendanceStatuses",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AttendanceStatuses_StatusId",
                schema: "Attendance",
                table: "Attendances",
                column: "StatusId",
                principalSchema: "Attendance",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                schema: "Employee",
                table: "Employees",
                column: "ContractTypeId",
                principalSchema: "Employee",
                principalTable: "ContractTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatuses_StatusId",
                schema: "Employee",
                table: "Employees",
                column: "StatusId",
                principalSchema: "Employee",
                principalTable: "EmployeeStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Genders_GenderId",
                schema: "Employee",
                table: "Employees",
                column: "GenderId",
                principalSchema: "Employee",
                principalTable: "Genders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_MaritalStatuses_MaritalStatusId",
                schema: "Employee",
                table: "Employees",
                column: "MaritalStatusId",
                principalSchema: "Employee",
                principalTable: "MaritalStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId",
                schema: "Performance",
                table: "KPIEvaluations",
                column: "KPIId",
                principalSchema: "Performance",
                principalTable: "KPIs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId1",
                schema: "Performance",
                table: "KPIEvaluations",
                column: "KPIId1",
                principalSchema: "Performance",
                principalTable: "KPIs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "StatusId",
                principalSchema: "Payroll",
                principalTable: "PayrollStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_AttendanceStatuses_StatusId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeStatuses_StatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Genders_GenderId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_MaritalStatuses_MaritalStatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId",
                schema: "Performance",
                table: "KPIEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId1",
                schema: "Performance",
                table: "KPIEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropIndex(
                name: "IX_PayrollStatuses_TenantId_NameEn",
                schema: "Payroll",
                table: "PayrollStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MaritalStatuses_TenantId_NameEn",
                schema: "Employee",
                table: "MaritalStatuses");

            migrationBuilder.DropIndex(
                name: "IX_KPIEvaluations_KPIId1",
                schema: "Performance",
                table: "KPIEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_Genders_TenantId_NameEn",
                schema: "Employee",
                table: "Genders");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeStatuses_TenantId_NameEn",
                schema: "Employee",
                table: "EmployeeStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Countries_Code",
                schema: "Organization",
                table: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_ContractTypes_TenantId_NameEn",
                schema: "Employee",
                table: "ContractTypes");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceStatuses_TenantId_NameEn",
                schema: "Attendance",
                table: "AttendanceStatuses");

            migrationBuilder.DropColumn(
                name: "KPIId1",
                schema: "Performance",
                table: "KPIEvaluations");

            migrationBuilder.EnsureSchema(
                name: "securety");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                schema: "security",
                newName: "UserTokens",
                newSchema: "securety");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                schema: "security",
                newName: "UserRoles",
                newSchema: "securety");

            migrationBuilder.RenameTable(
                name: "UserLogins",
                schema: "security",
                newName: "UserLogins",
                newSchema: "securety");

            migrationBuilder.RenameTable(
                name: "UserClaims",
                schema: "security",
                newName: "UserClaims",
                newSchema: "securety");

            migrationBuilder.RenameTable(
                name: "SalaryDeductions",
                schema: "Payroll",
                newName: "SalaryDeductions");

            migrationBuilder.RenameTable(
                name: "SalaryAllowances",
                schema: "Payroll",
                newName: "SalaryAllowances");

            migrationBuilder.RenameTable(
                name: "RoleClaims",
                schema: "security",
                newName: "RoleClaims",
                newSchema: "securety");

            migrationBuilder.RenameTable(
                name: "PayslipDeductions",
                schema: "Payroll",
                newName: "PayslipDeductions");

            migrationBuilder.RenameTable(
                name: "PayslipAllowances",
                schema: "Payroll",
                newName: "PayslipAllowances");

            migrationBuilder.RenameTable(
                name: "PayrollStatuses",
                schema: "Payroll",
                newName: "PayrollStatuses");

            migrationBuilder.RenameTable(
                name: "OrganizationInvoiceItems",
                schema: "Organization",
                newName: "OrganizationInvoiceItems");

            migrationBuilder.RenameTable(
                name: "MaritalStatuses",
                schema: "Employee",
                newName: "MaritalStatuses");

            migrationBuilder.RenameTable(
                name: "KPIEvaluations",
                schema: "Performance",
                newName: "KPIEvaluations");

            migrationBuilder.RenameTable(
                name: "Goals",
                schema: "Performance",
                newName: "Goals");

            migrationBuilder.RenameTable(
                name: "GoalMilestones",
                schema: "Performance",
                newName: "GoalMilestones");

            migrationBuilder.RenameTable(
                name: "Genders",
                schema: "Employee",
                newName: "Genders");

            migrationBuilder.RenameTable(
                name: "EmployeeStatuses",
                schema: "Employee",
                newName: "EmployeeStatuses");

            migrationBuilder.RenameTable(
                name: "Countries",
                schema: "Organization",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "ContractTypes",
                schema: "Employee",
                newName: "ContractTypes");

            migrationBuilder.RenameTable(
                name: "AttendanceStatuses",
                schema: "Attendance",
                newName: "AttendanceStatuses");

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId1",
                schema: "Organization",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageValue",
                table: "SalaryDeductions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRecurring",
                table: "SalaryDeductions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPercentage",
                table: "SalaryDeductions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SalaryDeductions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PercentageValue",
                table: "SalaryAllowances",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTaxable",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSubjectToInsurance",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPercentage",
                table: "SalaryAllowances",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SalaryAllowances",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeductionNameEn",
                table: "PayslipDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DeductionNameAr",
                table: "PayslipDeductions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AllowanceNameEn",
                table: "PayslipAllowances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AllowanceNameAr",
                table: "PayslipAllowances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "PayrollStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "PayrollStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PayrollStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "PayrollStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PayrollStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "PayrollStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "OrganizationInvoiceItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionEn",
                table: "OrganizationInvoiceItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "DescriptionAr",
                table: "OrganizationInvoiceItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "MaritalStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "MaritalStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "MaritalStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "MaritalStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "KPIEvaluations",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Evidence",
                table: "KPIEvaluations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "KPIEvaluations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TitleEn",
                table: "GoalMilestones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "TitleAr",
                table: "GoalMilestones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "GoalMilestones",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "GoalMilestones",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "Genders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Genders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Genders",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Genders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "EmployeeStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "EmployeeStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "EmployeeStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "EmployeeStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "EmployeeStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "EmployeeStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneCode",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Countries",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "Countries",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "ContractTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "ContractTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ContractTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "ContractTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ContractTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameEn",
                table: "AttendanceStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "NameAr",
                table: "AttendanceStatuses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AttendanceStatuses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "AttendanceStatuses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AttendanceStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ColorCode",
                table: "AttendanceStatuses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CountryId1",
                schema: "Organization",
                table: "Branches",
                column: "CountryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AttendanceStatuses_StatusId",
                schema: "Attendance",
                table: "Attendances",
                column: "StatusId",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Countries_CountryId1",
                schema: "Organization",
                table: "Branches",
                column: "CountryId1",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ContractTypes_ContractTypeId",
                schema: "Employee",
                table: "Employees",
                column: "ContractTypeId",
                principalTable: "ContractTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeStatuses_StatusId",
                schema: "Employee",
                table: "Employees",
                column: "StatusId",
                principalTable: "EmployeeStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Genders_GenderId",
                schema: "Employee",
                table: "Employees",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_MaritalStatuses_MaritalStatusId",
                schema: "Employee",
                table: "Employees",
                column: "MaritalStatusId",
                principalTable: "MaritalStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KPIEvaluations_KPIs_KPIId",
                table: "KPIEvaluations",
                column: "KPIId",
                principalSchema: "Performance",
                principalTable: "KPIs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "StatusId",
                principalTable: "PayrollStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
