using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLastLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_BranchId_RequestType",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequests_EmployeeId_RequestType",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRequestOptions_TenantId_RequestType_NameEn",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_TenantId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.DropColumn(
                name: "RequestType",
                schema: "Requests",
                table: "EmployeeRequests");

            migrationBuilder.DropColumn(
                name: "RequestType",
                schema: "Requests",
                table: "EmployeeRequestOptions");

            migrationBuilder.DropColumn(
                name: "RequestType",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastLogin",
                schema: "security",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogin",
                schema: "security",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                schema: "Requests",
                table: "EmployeeRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestType",
                schema: "Requests",
                table: "BranchRequestSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                name: "IX_EmployeeRequestOptions_TenantId_RequestType_NameEn",
                schema: "Requests",
                table: "EmployeeRequestOptions",
                columns: new[] { "TenantId", "RequestType", "NameEn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "BranchId", "RequestType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_TenantId_RequestType",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "TenantId", "RequestType" });
        }
    }
}
