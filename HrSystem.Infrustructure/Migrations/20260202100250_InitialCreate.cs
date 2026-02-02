using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Payroll");

            migrationBuilder.EnsureSchema(
                name: "Attendance");

            migrationBuilder.EnsureSchema(
                name: "Organization");

            migrationBuilder.EnsureSchema(
                name: "Requests");

            migrationBuilder.EnsureSchema(
                name: "Employee");

            migrationBuilder.EnsureSchema(
                name: "Lifecycle");

            migrationBuilder.EnsureSchema(
                name: "Performance");

            migrationBuilder.EnsureSchema(
                name: "Leave");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "AllowanceTypes",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    IsSubjectToInsurance = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AllowanceTypes", x => x.Id);
                });

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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "DeductionTypes",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_DeductionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRequestOptions",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeRequestOptions", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "FeedbackTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAnonymousAllowed = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_FeedbackTypes", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "GoalPriorities",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_GoalPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GoalStatuses",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_GoalStatuses", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "MiscellaneousTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MiscellaneousTypes", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "OvertimeTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultMultiplier = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OvertimeTypes", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                name: "PermissionTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxHoursPerRequest = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MaxHoursPerMonth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DeductsFromLeave = table.Column<bool>(type: "bit", nullable: false),
                    HoursPerLeaveDay = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PermissionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PersonalTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicHolidays",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PublicHolidays", x => x.Id);
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialInsuranceRates",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    EmployeeRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EmployerRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MinSalaryBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxSalaryBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_SocialInsuranceRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaxEmployees = table.Column<int>(type: "int", nullable: false),
                    MaxStorageGB = table.Column<int>(type: "int", nullable: false),
                    MaxDepartments = table.Column<int>(type: "int", nullable: false),
                    AllowBiometricIntegration = table.Column<bool>(type: "bit", nullable: false),
                    AllowPayrollModule = table.Column<bool>(type: "bit", nullable: false),
                    AllowPerformanceModule = table.Column<bool>(type: "bit", nullable: false),
                    AllowRecruitmentModule = table.Column<bool>(type: "bit", nullable: false),
                    AllowCustomReports = table.Column<bool>(type: "bit", nullable: false),
                    AllowAPIAccess = table.Column<bool>(type: "bit", nullable: false),
                    TrialDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxBrackets",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    MinIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_TaxBrackets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresBudgetApproval = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_TrainingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacationTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    MaxDaysPerYear = table.Column<int>(type: "int", nullable: true),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_VacationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingHoursPerDay = table.Column<int>(type: "int", nullable: false),
                    WorkingDaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    GracePeriodLate = table.Column<TimeSpan>(type: "time", nullable: true),
                    GracePeriodEarlyLeave = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsSaturday = table.Column<bool>(type: "bit", nullable: false),
                    IsSunday = table.Column<bool>(type: "bit", nullable: false),
                    IsMonday = table.Column<bool>(type: "bit", nullable: false),
                    IsTuesday = table.Column<bool>(type: "bit", nullable: false),
                    IsWednesday = table.Column<bool>(type: "bit", nullable: false),
                    IsThursday = table.Column<bool>(type: "bit", nullable: false),
                    IsFriday = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_WorkSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeavePolicies",
                schema: "Leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultDaysPerYear = table.Column<int>(type: "int", nullable: false),
                    MaxCarryForward = table.Column<int>(type: "int", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    RequiresHRApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    MaxConsecutiveDays = table.Column<int>(type: "int", nullable: false),
                    MinDaysNotice = table.Column<int>(type: "int", nullable: false),
                    RequiresDocument = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeaveTypeId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_LeavePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeavePolicies_LeaveTypes_LeaveTypeId1",
                        column: x => x.LeaveTypeId1,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayrollCycles",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CycleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalGrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalInsurance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PayrollCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollCycles_PayrollStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "PayrollStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "security",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Industry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommercialRegistrationNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TaxRegistrationNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LegalEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsTrialPeriod = table.Column<bool>(type: "bit", nullable: false),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxEmployees = table.Column<int>(type: "int", nullable: false),
                    CurrentEmployeeCount = table.Column<int>(type: "int", nullable: false),
                    MaxStorageGB = table.Column<int>(type: "int", nullable: false),
                    CurrentStorageGB = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekStartDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalSchema: "Organization",
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationInvoices",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OrganizationInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationInvoices_InvoiceStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Organization",
                        principalTable: "InvoiceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationInvoices_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OrganizationInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationInvoiceItems_OrganizationInvoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "Organization",
                        principalTable: "OrganizationInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "security",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsConfigurationRecord = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInDeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckOutDeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkedHours = table.Column<TimeSpan>(type: "time", nullable: true),
                    OvertimeHours = table.Column<TimeSpan>(type: "time", nullable: true),
                    LateMinutes = table.Column<TimeSpan>(type: "time", nullable: true),
                    EarlyLeaveMinutes = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsLate = table.Column<bool>(type: "bit", nullable: false),
                    IsEarlyLeave = table.Column<bool>(type: "bit", nullable: false),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkShift = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    WorkDays = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    GracePeriod = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MaxLatePerMonth = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OvertimeEligible = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OvertimeCalculation = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AttendanceMethod = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LateDeductionPolicy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AbsenceDeductionPolicy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HalfDayRule = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    MissingCheckoutHandling = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_AttendanceStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "AttendanceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AddressAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddressEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsHeadquarter = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxEmployeeCapacity = table.Column<int>(type: "int", nullable: true),
                    CurrentEmployeeCount = table.Column<int>(type: "int", nullable: false),
                    WorkStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingDays = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CountryId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Countries_CountryId1",
                        column: x => x.CountryId1,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Branches_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchHolidays",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringMonth = table.Column<int>(type: "int", nullable: true),
                    RecurringDay = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BranchHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchHolidays_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchRequestSettings",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    IsVisibleToEmployees = table.Column<bool>(type: "bit", nullable: false),
                    AllowEmployeesToSubmit = table.Column<bool>(type: "bit", nullable: false),
                    RequireAttachment = table.Column<bool>(type: "bit", nullable: false),
                    MaxOpenRequests = table.Column<int>(type: "int", nullable: true),
                    CustomInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BranchRequestSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchRequestSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchWorkSchedules",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingHoursPerDay = table.Column<int>(type: "int", nullable: false),
                    WorkingDaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    GracePeriodLate = table.Column<TimeSpan>(type: "time", nullable: true),
                    GracePeriodEarlyLeave = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsSunday = table.Column<bool>(type: "bit", nullable: false),
                    IsMonday = table.Column<bool>(type: "bit", nullable: false),
                    IsTuesday = table.Column<bool>(type: "bit", nullable: false),
                    IsWednesday = table.Column<bool>(type: "bit", nullable: false),
                    IsThursday = table.Column<bool>(type: "bit", nullable: false),
                    IsFriday = table.Column<bool>(type: "bit", nullable: false),
                    IsSaturday = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BranchWorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchWorkSchedules_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobTitles",
                schema: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    MinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_JobTitles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobTitles_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobTitles_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                schema: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalSchema: "Employee",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaritalStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DirectManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HiringDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProbationEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProbationPeriodMonths = table.Column<int>(type: "int", nullable: true),
                    TerminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_ContractTypes_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalTable: "ContractTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "Employee",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_EmployeeStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "EmployeeStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_DirectManagerId",
                        column: x => x.DirectManagerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_JobTitles_JobTitleId",
                        column: x => x.JobTitleId,
                        principalSchema: "Employee",
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_MaritalStatuses_MaritalStatusId",
                        column: x => x.MaritalStatusId,
                        principalTable: "MaritalStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPIs",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    MeasurementCriteria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_KPIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPIs_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "Employee",
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KPIs_JobTitles_JobTitleId",
                        column: x => x.JobTitleId,
                        principalSchema: "Employee",
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAssets",
                schema: "Lifecycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReturnNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAssets_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeBiometrics",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BiometricType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TemplateData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EnrolledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeBiometrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeBiometrics_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDocuments",
                schema: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDocuments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeWorkSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeWorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkSchedules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkSchedules_WorkSchedules_WorkScheduleId",
                        column: x => x.WorkScheduleId,
                        principalSchema: "Attendance",
                        principalTable: "WorkSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PriorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Goals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Goals_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goals_GoalPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalSchema: "Performance",
                        principalTable: "GoalPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Goals_GoalStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Performance",
                        principalTable: "GoalStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                schema: "Leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CarriedForwardDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_LeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalSchema: "Leave",
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "Leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LeaveStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HRApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HRApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HRComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalSchema: "Leave",
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveStatuses_LeaveStatusId",
                        column: x => x.LeaveStatusId,
                        principalTable: "LeaveStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentMonths = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OffboardingTasks",
                schema: "Lifecycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaskNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OffboardingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OffboardingTasks_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingTasks",
                schema: "Lifecycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaskNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OnboardingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingTasks_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeRequests",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hours = table.Column<TimeSpan>(type: "time", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OvertimeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OvertimeRequests_OvertimeStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Attendance",
                        principalTable: "OvertimeStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payslips",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayrollCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayslipNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocialInsuranceEmployee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocialInsuranceEmployer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LeaveDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<int>(type: "int", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWorkingDays = table.Column<int>(type: "int", nullable: false),
                    ActualWorkingDays = table.Column<int>(type: "int", nullable: false),
                    AbsentDays = table.Column<int>(type: "int", nullable: false),
                    PdfFilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfFileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Payslips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payslips_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payslips_PayrollCycles_PayrollCycleId",
                        column: x => x.PayrollCycleId,
                        principalSchema: "Payroll",
                        principalTable: "PayrollCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReviews",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StrengthsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrengthsEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeaknessesAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeaknessesEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImprovementAreasAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImprovementAreasEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentsAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentsEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeAcknowledgedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeComments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PerformanceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_Employees_ReviewerId",
                        column: x => x.ReviewerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_ReviewStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Performance",
                        principalTable: "ReviewStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerformanceReviews_ReviewTypes_ReviewTypeId",
                        column: x => x.ReviewTypeId,
                        principalSchema: "Performance",
                        principalTable: "ReviewTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PolicyAcknowledgments",
                schema: "Lifecycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PolicyVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AcknowledgedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DocumentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeSignature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PolicyAcknowledgments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAcknowledgments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Salaries",
                schema: "Payroll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSocialInsuranceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SocialInsuranceEmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SocialInsuranceEmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankBranch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankIban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankSwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Salaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Salaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoalMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_GoalMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalMilestones_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayslipAllowances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayslipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowanceNameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AllowanceNameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PayslipAllowances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipAllowances_AllowanceTypes_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "Payroll",
                        principalTable: "AllowanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayslipAllowances_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalSchema: "Payroll",
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayslipDeductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayslipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeductionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeductionNameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeductionNameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PayslipDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayslipDeductions_DeductionTypes_DeductionTypeId",
                        column: x => x.DeductionTypeId,
                        principalSchema: "Payroll",
                        principalTable: "DeductionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayslipDeductions_Payslips_PayslipId",
                        column: x => x.PayslipId,
                        principalSchema: "Payroll",
                        principalTable: "Payslips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                schema: "Performance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvidedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedbackType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Employees_ProvidedBy",
                        column: x => x.ProvidedBy,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_PerformanceReviews_PerformanceReviewId",
                        column: x => x.PerformanceReviewId,
                        principalSchema: "Performance",
                        principalTable: "PerformanceReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KPIEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceReviewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KPIId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeightedScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_KPIEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPIEvaluations_KPIs_KPIId",
                        column: x => x.KPIId,
                        principalSchema: "Performance",
                        principalTable: "KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KPIEvaluations_PerformanceReviews_PerformanceReviewId",
                        column: x => x.PerformanceReviewId,
                        principalSchema: "Performance",
                        principalTable: "PerformanceReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryAllowances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    PercentageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_SalaryAllowances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryAllowances_AllowanceTypes_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "Payroll",
                        principalTable: "AllowanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalaryAllowances_Salaries_SalaryId",
                        column: x => x.SalaryId,
                        principalSchema: "Payroll",
                        principalTable: "Salaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalaryDeductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeductionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    PercentageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_SalaryDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryDeductions_DeductionTypes_DeductionTypeId",
                        column: x => x.DeductionTypeId,
                        principalSchema: "Payroll",
                        principalTable: "DeductionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalaryDeductions_Salaries_SalaryId",
                        column: x => x.SalaryId,
                        principalSchema: "Payroll",
                        principalTable: "Salaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRequests",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeRequestOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeRequests_EmployeeRequestOptions_EmployeeRequestOptionId",
                        column: x => x.EmployeeRequestOptionId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequestOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeRequests_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeRequests_Users_ProcessedBy",
                        column: x => x.ProcessedBy,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBranchRoles",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_UserBranchRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBranchRoles_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBranchRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedbackTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeedbackContent = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Rating = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TargetDepartment = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SuggestedImprovement = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResponseRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResponseContent = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_FeedbackRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackRequestDetails_FeedbackTypes_FeedbackTypeId",
                        column: x => x.FeedbackTypeId,
                        principalSchema: "Requests",
                        principalTable: "FeedbackTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MiscellaneousTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpectedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_MiscellaneousRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscellaneousRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MiscellaneousRequestDetails_MiscellaneousTypes_MiscellaneousTypeId",
                        column: x => x.MiscellaneousTypeId,
                        principalSchema: "Requests",
                        principalTable: "MiscellaneousTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OvertimeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OvertimeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedHours = table.Column<TimeSpan>(type: "time", nullable: false),
                    ActualHours = table.Column<TimeSpan>(type: "time", nullable: true),
                    Multiplier = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 1.5m),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaskDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_OvertimeRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OvertimeRequestDetails_OvertimeTypes_OvertimeTypeId",
                        column: x => x.OvertimeTypeId,
                        principalSchema: "Requests",
                        principalTable: "OvertimeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermissionRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ToTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    TotalHours = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LeaveDeduction = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PermissionRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissionRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissionRequestDetails_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionRequestDetails_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalSchema: "Leave",
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionRequestDetails_PermissionTypes_PermissionTypeId",
                        column: x => x.PermissionTypeId,
                        principalSchema: "Requests",
                        principalTable: "PermissionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonalRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonalTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresConfidentiality = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PreferredContactMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalContactInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_PersonalRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonalRequestDetails_PersonalTypes_PersonalTypeId",
                        column: x => x.PersonalTypeId,
                        principalSchema: "Requests",
                        principalTable: "PersonalTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TrainingProvider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrainingLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrainingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrainingEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "EGP"),
                    Objectives = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExpectedOutcome = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CertificationObtained = table.Column<bool>(type: "bit", nullable: true),
                    CertificateUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_TrainingRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrainingRequestDetails_TrainingTypes_TrainingTypeId",
                        column: x => x.TrainingTypeId,
                        principalSchema: "Requests",
                        principalTable: "TrainingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VacationRequestDetails",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeavePolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HRApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HRApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HRComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_VacationRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacationRequestDetails_EmployeeRequests_EmployeeRequestId",
                        column: x => x.EmployeeRequestId,
                        principalSchema: "Requests",
                        principalTable: "EmployeeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VacationRequestDetails_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VacationRequestDetails_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalSchema: "Leave",
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VacationRequestDetails_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VacationRequestDetails_VacationTypes_VacationTypeId",
                        column: x => x.VacationTypeId,
                        principalSchema: "Requests",
                        principalTable: "VacationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowanceTypes_IsDeleted",
                schema: "Payroll",
                table: "AllowanceTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_EmployeeId_Date_IsConfigurationRecord",
                schema: "Attendance",
                table: "Attendances",
                columns: new[] { "EmployeeId", "Date", "IsConfigurationRecord" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_IsDeleted",
                schema: "Attendance",
                table: "Attendances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StatusId",
                schema: "Attendance",
                table: "Attendances",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BranchManagerId",
                schema: "Organization",
                table: "Branches",
                column: "BranchManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "Organization",
                table: "Branches",
                column: "Code",
                unique: true);

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
                name: "IX_Branches_IsDeleted",
                schema: "Organization",
                table: "Branches",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_OrganizationId_IsActive",
                schema: "Organization",
                table: "Branches",
                columns: new[] { "OrganizationId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_BranchId",
                schema: "Organization",
                table: "BranchHolidays",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_BranchId_Date_Year",
                schema: "Organization",
                table: "BranchHolidays",
                columns: new[] { "BranchId", "Date", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_IsDeleted",
                schema: "Organization",
                table: "BranchHolidays",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "BranchId", "RequestType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_IsDeleted",
                schema: "Requests",
                table: "BranchRequestSettings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_TenantId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "TenantId", "RequestType" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkSchedules_BranchId",
                schema: "Organization",
                table: "BranchWorkSchedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkSchedules_IsDeleted",
                schema: "Organization",
                table: "BranchWorkSchedules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DeductionTypes_IsDeleted",
                schema: "Payroll",
                table: "DeductionTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId",
                schema: "Employee",
                table: "Departments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsDeleted",
                schema: "Employee",
                table: "Departments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                schema: "Employee",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId",
                schema: "Employee",
                table: "Departments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                schema: "Employee",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssets_EmployeeId",
                schema: "Lifecycle",
                table: "EmployeeAssets",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssets_IsDeleted",
                schema: "Lifecycle",
                table: "EmployeeAssets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBiometrics_EmployeeId_BiometricType",
                schema: "Attendance",
                table: "EmployeeBiometrics",
                columns: new[] { "EmployeeId", "BiometricType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBiometrics_IsDeleted",
                schema: "Attendance",
                table: "EmployeeBiometrics",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_DocumentType",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_EmployeeId",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_IsDeleted",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequestOptions_IsDeleted",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequestOptions_TenantId_RequestType_NameEn",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                columns: new[] { "TenantId", "RequestType", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_ApprovedBy",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_BranchId_RequestType",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "BranchId", "RequestType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_EmployeeId_RequestType",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "EmployeeId", "RequestType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "EmployeeRequestOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_IsDeleted",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_ProcessedBy",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_TenantId_Status",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                schema: "Employee",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ContractTypeId",
                schema: "Employee",
                table: "Employees",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                schema: "Employee",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DirectManagerId",
                schema: "Employee",
                table: "Employees",
                column: "DirectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                schema: "Employee",
                table: "Employees",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                schema: "Employee",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_GenderId",
                schema: "Employee",
                table: "Employees",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsDeleted",
                schema: "Employee",
                table: "Employees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobTitleId",
                schema: "Employee",
                table: "Employees",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_MaritalStatusId",
                schema: "Employee",
                table: "Employees",
                column: "MaritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_NationalId",
                schema: "Employee",
                table: "Employees",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_StatusId",
                schema: "Employee",
                table: "Employees",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkSchedules_EmployeeId",
                table: "EmployeeWorkSchedules",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkSchedules_IsDeleted",
                table: "EmployeeWorkSchedules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkSchedules_WorkScheduleId",
                table: "EmployeeWorkSchedules",
                column: "WorkScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "FeedbackRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackRequestDetails_FeedbackTypeId",
                schema: "Requests",
                table: "FeedbackRequestDetails",
                column: "FeedbackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackRequestDetails_IsDeleted",
                schema: "Requests",
                table: "FeedbackRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_IsDeleted",
                schema: "Performance",
                table: "Feedbacks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PerformanceReviewId",
                schema: "Performance",
                table: "Feedbacks",
                column: "PerformanceReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ProvidedBy",
                schema: "Performance",
                table: "Feedbacks",
                column: "ProvidedBy");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTypes_IsDeleted",
                schema: "Requests",
                table: "FeedbackTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTypes_TenantId_NameEn",
                schema: "Requests",
                table: "FeedbackTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_GoalId",
                table: "GoalMilestones",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalMilestones_IsDeleted",
                table: "GoalMilestones",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GoalPriorities_Code",
                schema: "Performance",
                table: "GoalPriorities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalPriorities_IsDeleted",
                schema: "Performance",
                table: "GoalPriorities",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_EmployeeId",
                table: "Goals",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_IsDeleted",
                table: "Goals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_PriorityId",
                table: "Goals",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_StatusId",
                table: "Goals",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GoalStatuses_Code",
                schema: "Performance",
                table: "GoalStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalStatuses_IsDeleted",
                schema: "Performance",
                table: "GoalStatuses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStatuses_Code",
                schema: "Organization",
                table: "InvoiceStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_BranchId",
                schema: "Employee",
                table: "JobTitles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_IsDeleted",
                schema: "Employee",
                table: "JobTitles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_OrganizationId",
                schema: "Employee",
                table: "JobTitles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIEvaluations_IsDeleted",
                table: "KPIEvaluations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_KPIEvaluations_KPIId",
                table: "KPIEvaluations",
                column: "KPIId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIEvaluations_PerformanceReviewId",
                table: "KPIEvaluations",
                column: "PerformanceReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_DepartmentId",
                schema: "Performance",
                table: "KPIs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_IsDeleted",
                schema: "Performance",
                table: "KPIs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_JobTitleId",
                schema: "Performance",
                table: "KPIs",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeavePolicyId_Year",
                schema: "Leave",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeavePolicyId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_IsDeleted",
                schema: "Leave",
                table: "LeaveBalances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_LeavePolicyId",
                schema: "Leave",
                table: "LeaveBalances",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_IsDeleted",
                schema: "Leave",
                table: "LeavePolicies",
                column: "IsDeleted");

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
                name: "IX_LeaveRequests_EmployeeId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_IsDeleted",
                schema: "Leave",
                table: "LeaveRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "LeavePolicyId");

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
                name: "IX_LeaveRequests_ManagerId",
                schema: "Leave",
                table: "LeaveRequests",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_EmployeeId",
                schema: "Payroll",
                table: "Loans",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_IsActive",
                schema: "Payroll",
                table: "Loans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_IsDeleted",
                schema: "Payroll",
                table: "Loans",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousRequestDetails_IsDeleted",
                schema: "Requests",
                table: "MiscellaneousRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousRequestDetails_MiscellaneousTypeId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails",
                column: "MiscellaneousTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousTypes_IsDeleted",
                schema: "Requests",
                table: "MiscellaneousTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousTypes_TenantId_NameEn",
                schema: "Requests",
                table: "MiscellaneousTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OffboardingTasks_EmployeeId",
                schema: "Lifecycle",
                table: "OffboardingTasks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OffboardingTasks_IsDeleted",
                schema: "Lifecycle",
                table: "OffboardingTasks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTasks_EmployeeId",
                schema: "Lifecycle",
                table: "OnboardingTasks",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingTasks_IsDeleted",
                schema: "Lifecycle",
                table: "OnboardingTasks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvoiceItems_InvoiceId",
                table: "OrganizationInvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvoices_InvoiceNumber",
                schema: "Organization",
                table: "OrganizationInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvoices_OrganizationId",
                schema: "Organization",
                table: "OrganizationInvoices",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInvoices_StatusId",
                schema: "Organization",
                table: "OrganizationInvoices",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Code",
                schema: "Organization",
                table: "Organizations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CommercialRegistrationNumber",
                schema: "Organization",
                table: "Organizations",
                column: "CommercialRegistrationNumber",
                unique: true,
                filter: "[CommercialRegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_SubscriptionPlanId",
                schema: "Organization",
                table: "Organizations",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TaxRegistrationNumber",
                schema: "Organization",
                table: "Organizations",
                column: "TaxRegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "OvertimeRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestDetails_IsDeleted",
                schema: "Requests",
                table: "OvertimeRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequestDetails_OvertimeTypeId",
                schema: "Requests",
                table: "OvertimeRequestDetails",
                column: "OvertimeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_EmployeeId",
                schema: "Attendance",
                table: "OvertimeRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_IsDeleted",
                schema: "Attendance",
                table: "OvertimeRequests",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRequests_StatusId",
                schema: "Attendance",
                table: "OvertimeRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeStatuses_Code",
                schema: "Attendance",
                table: "OvertimeStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_IsDeleted",
                schema: "Requests",
                table: "OvertimeTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_TenantId_NameEn",
                schema: "Requests",
                table: "OvertimeTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_IsDeleted",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_Month_Year",
                schema: "Payroll",
                table: "PayrollCycles",
                columns: new[] { "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollCycles_StatusId",
                schema: "Payroll",
                table: "PayrollCycles",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipAllowances_AllowanceTypeId",
                table: "PayslipAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipAllowances_PayslipId",
                table: "PayslipAllowances",
                column: "PayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDeductions_DeductionTypeId",
                table: "PayslipDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayslipDeductions_PayslipId",
                table: "PayslipDeductions",
                column: "PayslipId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId",
                schema: "Payroll",
                table: "Payslips",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_IsDeleted",
                schema: "Payroll",
                table: "Payslips",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayrollCycleId",
                schema: "Payroll",
                table: "Payslips",
                column: "PayrollCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_PayslipNumber",
                schema: "Payroll",
                table: "Payslips",
                column: "PayslipNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_EmployeeId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_IsDeleted",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReviews_ReviewerId",
                schema: "Performance",
                table: "PerformanceReviews",
                column: "ReviewerId");

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
                name: "IX_PermissionRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequestDetails_IsDeleted",
                schema: "Requests",
                table: "PermissionRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequestDetails_LeavePolicyId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequestDetails_ManagerId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRequestDetails_PermissionTypeId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                column: "PermissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTypes_IsDeleted",
                schema: "Requests",
                table: "PermissionTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PermissionTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "PersonalRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRequestDetails_IsDeleted",
                schema: "Requests",
                table: "PersonalRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalRequestDetails_PersonalTypeId",
                schema: "Requests",
                table: "PersonalRequestDetails",
                column: "PersonalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTypes_IsDeleted",
                schema: "Requests",
                table: "PersonalTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PersonalTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAcknowledgments_EmployeeId_PolicyName_PolicyVersion",
                schema: "Lifecycle",
                table: "PolicyAcknowledgments",
                columns: new[] { "EmployeeId", "PolicyName", "PolicyVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAcknowledgments_IsDeleted",
                schema: "Lifecycle",
                table: "PolicyAcknowledgments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHolidays_Date_Year",
                schema: "Attendance",
                table: "PublicHolidays",
                columns: new[] { "Date", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicHolidays_IsDeleted",
                schema: "Attendance",
                table: "PublicHolidays",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsDeleted",
                schema: "security",
                table: "RefreshTokens",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "security",
                table: "RefreshTokens",
                column: "UserId");

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

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "security",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_EmployeeId",
                schema: "Payroll",
                table: "Salaries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_IsDeleted",
                schema: "Payroll",
                table: "Salaries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAllowances_AllowanceTypeId",
                table: "SalaryAllowances",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAllowances_IsDeleted",
                table: "SalaryAllowances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryAllowances_SalaryId",
                table: "SalaryAllowances",
                column: "SalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_DeductionTypeId",
                table: "SalaryDeductions",
                column: "DeductionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_IsDeleted",
                table: "SalaryDeductions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_SalaryId",
                table: "SalaryDeductions",
                column: "SalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialInsuranceRates_IsDeleted",
                schema: "Payroll",
                table: "SocialInsuranceRates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SocialInsuranceRates_Year",
                schema: "Payroll",
                table: "SocialInsuranceRates",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_Code",
                schema: "Organization",
                table: "SubscriptionPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxBrackets_IsDeleted",
                schema: "Payroll",
                table: "TaxBrackets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "TrainingRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequestDetails_IsDeleted",
                schema: "Requests",
                table: "TrainingRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequestDetails_TrainingTypeId",
                schema: "Requests",
                table: "TrainingRequestDetails",
                column: "TrainingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingTypes_IsDeleted",
                schema: "Requests",
                table: "TrainingTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingTypes_TenantId_NameEn",
                schema: "Requests",
                table: "TrainingTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBranchRoles_BranchId",
                schema: "security",
                table: "UserBranchRoles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBranchRoles_UserId_BranchId_RoleName",
                schema: "security",
                table: "UserBranchRoles",
                columns: new[] { "UserId", "BranchId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "security",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                schema: "security",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                schema: "security",
                table: "Users",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                schema: "security",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "security",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_EmployeeRequestId",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "EmployeeRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_IsDeleted",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_LeavePolicyId",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "LeavePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_LeaveTypeId",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_ManagerId",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationRequestDetails_VacationTypeId",
                schema: "Requests",
                table: "VacationRequestDetails",
                column: "VacationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VacationTypes_IsDeleted",
                schema: "Requests",
                table: "VacationTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VacationTypes_TenantId_NameEn",
                schema: "Requests",
                table: "VacationTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_IsDeleted",
                schema: "Attendance",
                table: "WorkSchedules",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_Users_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalSchema: "security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_Users_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalSchema: "security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Users_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalSchema: "security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_Users_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalSchema: "security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                schema: "Attendance",
                table: "Attendances",
                column: "EmployeeId",
                principalSchema: "Employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Employees_BranchManagerId",
                schema: "Organization",
                table: "Branches",
                column: "BranchManagerId",
                principalSchema: "Employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                schema: "Employee",
                table: "Departments",
                column: "ManagerId",
                principalSchema: "Employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Employees_BranchManagerId",
                schema: "Organization",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attendances",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "BranchHolidays",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "BranchRequestSettings",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "BranchWorkSchedules",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "EmployeeAssets",
                schema: "Lifecycle");

            migrationBuilder.DropTable(
                name: "EmployeeBiometrics",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "EmployeeDocuments",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "EmployeeWorkSchedules");

            migrationBuilder.DropTable(
                name: "FeedbackRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "Feedbacks",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "GoalMilestones");

            migrationBuilder.DropTable(
                name: "KPIEvaluations");

            migrationBuilder.DropTable(
                name: "LeaveBalances",
                schema: "Leave");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "Leave");

            migrationBuilder.DropTable(
                name: "Loans",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "MiscellaneousRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OffboardingTasks",
                schema: "Lifecycle");

            migrationBuilder.DropTable(
                name: "OnboardingTasks",
                schema: "Lifecycle");

            migrationBuilder.DropTable(
                name: "OrganizationInvoiceItems");

            migrationBuilder.DropTable(
                name: "OvertimeRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OvertimeRequests",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "PayslipAllowances");

            migrationBuilder.DropTable(
                name: "PayslipDeductions");

            migrationBuilder.DropTable(
                name: "PermissionRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "PersonalRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "PolicyAcknowledgments",
                schema: "Lifecycle");

            migrationBuilder.DropTable(
                name: "PublicHolidays",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "security");

            migrationBuilder.DropTable(
                name: "SalaryAllowances");

            migrationBuilder.DropTable(
                name: "SalaryDeductions");

            migrationBuilder.DropTable(
                name: "SocialInsuranceRates",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "TaxBrackets",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "TrainingRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "UserBranchRoles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "VacationRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "AttendanceStatuses");

            migrationBuilder.DropTable(
                name: "WorkSchedules",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "FeedbackTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "KPIs",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "PerformanceReviews",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "LeaveStatuses");

            migrationBuilder.DropTable(
                name: "MiscellaneousTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OrganizationInvoices",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "OvertimeTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OvertimeStatuses",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "Payslips",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "PermissionTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "PersonalTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "AllowanceTypes",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "DeductionTypes",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "Salaries",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "TrainingTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "EmployeeRequests",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "LeavePolicies",
                schema: "Leave");

            migrationBuilder.DropTable(
                name: "VacationTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "GoalPriorities",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "GoalStatuses",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "ReviewStatuses",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "ReviewTypes",
                schema: "Performance");

            migrationBuilder.DropTable(
                name: "InvoiceStatuses",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "PayrollCycles",
                schema: "Payroll");

            migrationBuilder.DropTable(
                name: "EmployeeRequestOptions",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "security");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropTable(
                name: "PayrollStatuses");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropTable(
                name: "Departments",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "EmployeeStatuses");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "JobTitles",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "MaritalStatuses");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans",
                schema: "Organization");
        }
    }
}
