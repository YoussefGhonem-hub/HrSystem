using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeDocumentTypeEnumConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_EmployeeDocumentTypes_DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeDocumentTypes",
                schema: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                schema: "Employee",
                table: "EmployeeDocuments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_DocumentType",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "DocumentType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeDocuments_DocumentType",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "EmployeeDocumentTypes",
                schema: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocuments_DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocumentTypes_CategoryKey",
                schema: "Employee",
                table: "EmployeeDocumentTypes",
                column: "CategoryKey");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocumentTypes_DisplayOrder",
                schema: "Employee",
                table: "EmployeeDocumentTypes",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDocumentTypes_IsActive",
                schema: "Employee",
                table: "EmployeeDocumentTypes",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDocuments_EmployeeDocumentTypes_DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments",
                column: "DocumentTypeId",
                principalSchema: "Employee",
                principalTable: "EmployeeDocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
