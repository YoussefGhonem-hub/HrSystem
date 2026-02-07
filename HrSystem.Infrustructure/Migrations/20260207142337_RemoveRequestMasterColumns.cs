using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequestMasterColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRequests_EmployeeRequestOptions_EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropTable(
                name: "EmployeeRequestOptions",
                schema: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_VacationTypes_TenantId_NameEn",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropIndex(
                name: "IX_TrainingTypes_TenantId_NameEn",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropIndex(
                name: "IX_RequestTypes_TenantId_Code",
                schema: "Requests",
                table: "RequestTypes");

            migrationBuilder.DropIndex(
                name: "IX_PersonalTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropIndex(
                name: "IX_PermissionTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_OvertimeTypes_TenantId_NameEn",
                schema: "Requests",
                table: "OvertimeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MiscellaneousTypes_TenantId_NameEn",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropIndex(
                name: "IX_FeedbackTypes_TenantId_NameEn",
                schema: "Requests",
                table: "FeedbackTypes");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropColumn(
                name: "MaxDaysPerYear",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropColumn(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "VacationRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "VacationRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropColumn(
                name: "RequiresBudgetApproval",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "TrainingRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "TrainingRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "RequestTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "RequestTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropColumn(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "PersonalRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "PersonalRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "PermissionRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "PermissionRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "OvertimeTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "OvertimeTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "OvertimeRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "OvertimeRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropColumn(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "FeedbackTypes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "FeedbackTypes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Requests",
                table: "FeedbackRequestDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "Requests",
                table: "FeedbackRequestDetails");

            migrationBuilder.DropColumn(
                name: "EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.CreateIndex(
                name: "IX_VacationTypes_NameEn",
                schema: "Requests",
                table: "VacationTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingTypes_NameEn",
                schema: "Requests",
                table: "TrainingTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypes_Code",
                schema: "Requests",
                table: "RequestTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTypes_NameEn",
                schema: "Requests",
                table: "PersonalTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTypes_NameEn",
                schema: "Requests",
                table: "PermissionTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_NameEn",
                schema: "Requests",
                table: "OvertimeTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousTypes_NameEn",
                schema: "Requests",
                table: "MiscellaneousTypes",
                column: "NameEn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTypes_NameEn",
                schema: "Requests",
                table: "FeedbackTypes",
                column: "NameEn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VacationTypes_NameEn",
                schema: "Requests",
                table: "VacationTypes");

            migrationBuilder.DropIndex(
                name: "IX_TrainingTypes_NameEn",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropIndex(
                name: "IX_RequestTypes_Code",
                schema: "Requests",
                table: "RequestTypes");

            migrationBuilder.DropIndex(
                name: "IX_PersonalTypes_NameEn",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropIndex(
                name: "IX_PermissionTypes_NameEn",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropIndex(
                name: "IX_OvertimeTypes_NameEn",
                schema: "Requests",
                table: "OvertimeTypes");

            migrationBuilder.DropIndex(
                name: "IX_MiscellaneousTypes_NameEn",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropIndex(
                name: "IX_FeedbackTypes_NameEn",
                schema: "Requests",
                table: "FeedbackTypes");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "VacationTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxDaysPerYear",
                schema: "Requests",
                table: "VacationTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "VacationTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "VacationTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "VacationRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "VacationRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "TrainingTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresBudgetApproval",
                schema: "Requests",
                table: "TrainingTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "TrainingTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "TrainingRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "TrainingRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "RequestTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "RequestTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "PersonalTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "PersonalTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "PersonalTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "PersonalRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "PersonalRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "PermissionTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "PermissionTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "PermissionRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "OvertimeTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "OvertimeTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "OvertimeRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "OvertimeRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "MiscellaneousTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAttachment",
                schema: "Requests",
                table: "MiscellaneousTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "MiscellaneousTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "MiscellaneousRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "FeedbackTypes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "FeedbackTypes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Requests",
                table: "FeedbackRequestDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "Requests",
                table: "FeedbackRequestDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeRequestOptions",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRequestOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeRequestOptions_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalSchema: "Requests",
                        principalTable: "RequestTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VacationTypes_TenantId_NameEn",
                schema: "Requests",
                table: "VacationTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingTypes_TenantId_NameEn",
                schema: "Requests",
                table: "TrainingTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestTypes_TenantId_Code",
                schema: "Requests",
                table: "RequestTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PersonalTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissionTypes_TenantId_NameEn",
                schema: "Requests",
                table: "PermissionTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeTypes_TenantId_NameEn",
                schema: "Requests",
                table: "OvertimeTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousTypes_TenantId_NameEn",
                schema: "Requests",
                table: "MiscellaneousTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackTypes_TenantId_NameEn",
                schema: "Requests",
                table: "FeedbackTypes",
                columns: new[] { "TenantId", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "EmployeeRequestOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequestOptions_IsDeleted",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequestOptions_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequestOptions_TenantId_RequestTypeId_NameEn",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                columns: new[] { "TenantId", "RequestTypeId", "NameEn" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRequests_EmployeeRequestOptions_EmployeeRequestOptionId",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "EmployeeRequestOptionId",
                principalSchema: "Requests",
                principalTable: "EmployeeRequestOptions",
                principalColumn: "Id");
        }
    }
}
