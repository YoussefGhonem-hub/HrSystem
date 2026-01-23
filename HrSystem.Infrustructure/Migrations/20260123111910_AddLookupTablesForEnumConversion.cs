using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTablesForEnumConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeavePolicies_LeaveType",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropIndex(
                name: "IX_Branches_Country",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ReviewType",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Attendance",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Organization",
                table: "OrganizationInvoices");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "ContractType",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.RenameTable(
                name: "GoalStatuses",
                newName: "GoalStatuses",
                newSchema: "Performance");

            migrationBuilder.RenameTable(
                name: "GoalPriorities",
                newName: "GoalPriorities",
                newSchema: "Performance");

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Performance",
                table: "PerformanceReviews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Attendance",
                table: "OvertimeRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Organization",
                table: "OrganizationInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractTypeId",
                schema: "Employee",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GenderId",
                schema: "Employee",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MaritalStatusId",
                schema: "Employee",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Employee",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                schema: "Organization",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId1",
                schema: "Organization",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatusId",
                schema: "Attendance",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Performance",
                table: "GoalStatuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                schema: "Performance",
                table: "GoalStatuses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Performance",
                table: "GoalPriorities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                schema: "Performance",
                table: "GoalPriorities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceStatuses",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritalStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeStatuses",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewStatuses",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewTypes",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "ReviewTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_StatusId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_StatusId",
                schema: "Attendance",
                table: "OvertimeRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvoices_StatusId",
                schema: "Organization",
                table: "OrganizationInvoices",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "LeaveStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies",
                column: "LeaveTypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies",
                column: "LeaveTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ContractTypeId",
                schema: "Employee",
                table: "Employees",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_GenderId",
                schema: "Employee",
                table: "Employees",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_MaritalStatusId",
                schema: "Employee",
                table: "Employees",
                column: "MaritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_StatusId",
                schema: "Employee",
                table: "Employees",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CountryId",
                schema: "Organization",
                table: "Branches",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CountryId1",
                schema: "Organization",
                table: "Branches",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StatusId",
                schema: "Attendance",
                table: "Attendances",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalStatuses_Code",
                schema: "Performance",
                table: "GoalStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalPriorities_Code",
                schema: "Performance",
                table: "GoalPriorities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatuses_Code",
                schema: "Organization",
                table: "InvoiceStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeStatuses_Code",
                schema: "Attendance",
                table: "OvertimeStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewStatuses_Code",
                schema: "Performance",
                table: "ReviewStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewTypes_Code",
                schema: "Performance",
                table: "ReviewTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_AttendanceStatuses_StatusId",
                schema: "Attendance",
                table: "Attendances",
                column: "StatusId",
                principalTable: "AttendanceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Countries_CountryId",
                schema: "Organization",
                table: "Branches",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies",
                column: "LeaveTypeId1",
                principalTable: "LeaveTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveStatuses_LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "LeaveStatusId",
                principalTable: "LeaveStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationInvoices_InvoiceStatuses_StatusId",
                schema: "Organization",
                table: "OrganizationInvoices",
                column: "StatusId",
                principalSchema: "Organization",
                principalTable: "InvoiceStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OvertimeRequests_OvertimeStatuses_StatusId",
                schema: "Attendance",
                table: "OvertimeRequests",
                column: "StatusId",
                principalSchema: "Attendance",
                principalTable: "OvertimeStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "StatusId",
                principalTable: "PayrollStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PerformanceReviews_ReviewStatuses_StatusId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "StatusId",
                principalSchema: "Performance",
                principalTable: "ReviewStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PerformanceReviews_ReviewTypes_ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "ReviewTypeId",
                principalSchema: "Performance",
                principalTable: "ReviewTypes",
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
                name: "FK_Branches_Countries_CountryId",
                schema: "Organization",
                table: "Branches");

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
                name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveStatuses_LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationInvoices_InvoiceStatuses_StatusId",
                schema: "Organization",
                table: "OrganizationInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_OvertimeRequests_OvertimeStatuses_StatusId",
                schema: "Attendance",
                table: "OvertimeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropForeignKey(
                name: "FK_PerformanceReviews_ReviewStatuses_StatusId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_PerformanceReviews_ReviewTypes_ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropTable(
                name: "AttendanceStatuses");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "EmployeeStatuses");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "InvoiceStatuses",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "LeaveStatuses");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "MaritalStatuses");

            migrationBuilder.DropTable(
                name: "OvertimeStatuses",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "PayrollStatuses");

            migrationBuilder.DropTable(
                name: "ReviewStatuses",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "ReviewTypes",
                schema: "Performance");

            migrationBuilder.DropIndex(
                name: "IX_PerformanceReviews_ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropIndex(
                name: "IX_PerformanceReviews_StatusId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropIndex(
                name: "IX_PayrollCycles_StatusId",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropIndex(
                name: "IX_OvertimeRequests_StatusId",
                schema: "Attendance",
                table: "OvertimeRequests");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationInvoices_StatusId",
                schema: "Organization",
                table: "OrganizationInvoices");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeavePolicies_LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropIndex(
                name: "IX_LeavePolicies_LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ContractTypeId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_GenderId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_MaritalStatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_StatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CountryId",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CountryId1",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_StatusId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_GoalStatuses_Code",
                schema: "Performance",
                table: "GoalStatuses");

            migrationBuilder.DropIndex(
                name: "IX_GoalPriorities_Code",
                schema: "Performance",
                table: "GoalPriorities");

            migrationBuilder.DropColumn(
                name: "ReviewTypeId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Performance",
                table: "PerformanceReviews");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Payroll",
                table: "PayrollCycles");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Attendance",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Organization",
                table: "OrganizationInvoices");

            migrationBuilder.DropColumn(
                name: "LeaveStatusId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                schema: "Leave",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "LeaveTypeId1",
                schema: "Leave",
                table: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "ContractTypeId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "GenderId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "MaritalStatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Performance",
                table: "GoalStatuses");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                schema: "Performance",
                table: "GoalStatuses");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Performance",
                table: "GoalPriorities");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                schema: "Performance",
                table: "GoalPriorities");

            migrationBuilder.RenameTable(
                name: "GoalStatuses",
                schema: "Performance",
                newName: "GoalStatuses");

            migrationBuilder.RenameTable(
                name: "GoalPriorities",
                schema: "Performance",
                newName: "GoalPriorities");

            migrationBuilder.AddColumn<string>(
                name: "ReviewType",
                schema: "Performance",
                table: "PerformanceReviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Performance",
                table: "PerformanceReviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Payroll",
                table: "PayrollCycles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Attendance",
                table: "OvertimeRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "Organization",
                table: "OrganizationInvoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                schema: "Leave",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Leave",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeaveType",
                schema: "Leave",
                table: "LeavePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                schema: "Employee",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                schema: "Employee",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                schema: "Employee",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Employee",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Country",
                schema: "Organization",
                table: "Branches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "Attendance",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_LeaveType",
                schema: "Leave",
                table: "LeavePolicies",
                column: "LeaveType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Country",
                schema: "Organization",
                table: "Branches",
                column: "Country");
        }
    }
}
