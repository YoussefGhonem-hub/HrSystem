using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTypeForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add nullable columns first to allow backfill without default GUIDs
            migrationBuilder.AddColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                type: "uniqueidentifier",
                nullable: true);

            // Ensure RequestTypes master has the legacy codes (defensive for existing DBs)
            migrationBuilder.Sql(@"
DECLARE @tenant UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Organization.Organizations ORDER BY CreatedDate);
IF @tenant IS NULL SET @tenant = '00000000-0000-0000-0000-000000000000';

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Vacation', 'Vacation', N'إجازة', 'Days-based leave requests', 1, 1, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Vacation');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'OverTime', 'Overtime', N'وقت إضافي', 'Overtime work requests', 1, 2, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'OverTime');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Training', 'Training', N'تدريب', 'Training requests', 1, 3, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Training');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Miscellaneous', 'Miscellaneous', N'متنوع', 'General purpose requests', 1, 4, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Miscellaneous');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Personal', 'Personal', N'شخصي', 'Personal requests', 1, 5, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Personal');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Feedback', 'Feedback', N'ملاحظات', 'Feedback submissions', 1, 6, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Feedback');

INSERT INTO Requests.RequestTypes (Id, Code, NameEn, NameAr, Description, IsActive, SortOrder, TenantId, BranchId, CreatedDate, IsDeleted)
SELECT NEWID(), 'Permission', 'Permission', N'إذن', 'Short absence / permission requests', 1, 7, @tenant, NULL, SYSDATETIMEOFFSET(), 0
WHERE NOT EXISTS (SELECT 1 FROM Requests.RequestTypes WHERE Code = 'Permission');

-- Backfill from legacy enum values to the new RequestTypes master
DECLARE @vac UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Vacation');
DECLARE @ot UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'OverTime');
DECLARE @tr UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Training');
DECLARE @misc UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Miscellaneous');
DECLARE @per UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Personal');
DECLARE @fb UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Feedback');
DECLARE @perm UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes WHERE Code = 'Permission');
DECLARE @fallback UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Requests.RequestTypes ORDER BY SortOrder);

UPDATE r
SET RequestTypeId = CASE ISNULL(r.RequestType, 0)
    WHEN 1 THEN @vac
    WHEN 2 THEN @ot
    WHEN 3 THEN @tr
    WHEN 4 THEN @misc
    WHEN 5 THEN @per
    WHEN 6 THEN @fb
    WHEN 7 THEN @perm
    ELSE @fallback END
FROM Requests.EmployeeRequests r;

UPDATE o
SET RequestTypeId = CASE ISNULL(o.RequestType, 0)
    WHEN 1 THEN @vac
    WHEN 2 THEN @ot
    WHEN 3 THEN @tr
    WHEN 4 THEN @misc
    WHEN 5 THEN @per
    WHEN 6 THEN @fb
    WHEN 7 THEN @perm
    ELSE @fallback END
FROM Requests.EmployeeRequestOptions o;

UPDATE b
SET RequestTypeId = CASE ISNULL(b.RequestType, 0)
    WHEN 1 THEN @vac
    WHEN 2 THEN @ot
    WHEN 3 THEN @tr
    WHEN 4 THEN @misc
    WHEN 5 THEN @per
    WHEN 6 THEN @fb
    WHEN 7 THEN @perm
    ELSE @fallback END
FROM Requests.BranchRequestSettings b;

-- Fill any remaining nulls with fallback id
UPDATE Requests.EmployeeRequests SET RequestTypeId = @fallback WHERE RequestTypeId IS NULL;
UPDATE Requests.EmployeeRequestOptions SET RequestTypeId = @fallback WHERE RequestTypeId IS NULL;
UPDATE Requests.BranchRequestSettings SET RequestTypeId = @fallback WHERE RequestTypeId IS NULL;
");

            // Make columns non-nullable after backfill
            migrationBuilder.AlterColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_BranchId_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "BranchId", "RequestTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_EmployeeId_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                columns: new[] { "EmployeeId", "RequestTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRequests_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "RequestTypeId");

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

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "BranchId", "RequestTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_TenantId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "TenantId", "RequestTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchRequestSettings_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                column: "RequestTypeId",
                principalSchema: "Requests",
                principalTable: "RequestTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRequestOptions_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                column: "RequestTypeId",
                principalSchema: "Requests",
                principalTable: "RequestTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRequests_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests",
                column: "RequestTypeId",
                principalSchema: "Requests",
                principalTable: "RequestTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchRequestSettings_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRequestOptions_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRequests_RequestTypes_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_BranchId_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_EmployeeId_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequestOptions_RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequestOptions_TenantId_RequestTypeId_NameEn",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_TenantId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");
        }
    }
}
