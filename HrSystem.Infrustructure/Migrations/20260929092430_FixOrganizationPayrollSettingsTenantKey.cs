using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrganizationPayrollSettingsTenantKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationPayrollSettings_Organizations_OrganizationId",
                table: "OrganizationPayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationPayrollSettings_OrganizationId",
                table: "OrganizationPayrollSettings");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "OrganizationPayrollSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "OrganizationPayrollSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPayrollSettings_TenantId",
                table: "OrganizationPayrollSettings",
                column: "TenantId",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationPayrollSettings_Organizations_TenantId",
                table: "OrganizationPayrollSettings",
                column: "TenantId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationPayrollSettings_Organizations_TenantId",
                table: "OrganizationPayrollSettings");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationPayrollSettings_TenantId",
                table: "OrganizationPayrollSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "OrganizationPayrollSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "OrganizationPayrollSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationPayrollSettings_OrganizationId",
                table: "OrganizationPayrollSettings",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationPayrollSettings_Organizations_OrganizationId",
                table: "OrganizationPayrollSettings",
                column: "OrganizationId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
