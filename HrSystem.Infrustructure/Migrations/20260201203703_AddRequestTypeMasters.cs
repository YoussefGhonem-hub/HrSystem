using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTypeMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Requests");

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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                        name: "FK_VacationRequestDetails_VacationTypes_VacationTypeId",
                        column: x => x.VacationTypeId,
                        principalSchema: "Requests",
                        principalTable: "VacationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_EmployeeRequests_TenantId_Status",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "TenantId", "Status" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchRequestSettings",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "FeedbackRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "MiscellaneousRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OvertimeRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "PersonalRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "TrainingRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "VacationRequestDetails",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "FeedbackTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "MiscellaneousTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "OvertimeTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "PersonalTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "TrainingTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "EmployeeRequests",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "VacationTypes",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "EmployeeRequestOptions",
                schema: "Requests");
        }
    }
}
