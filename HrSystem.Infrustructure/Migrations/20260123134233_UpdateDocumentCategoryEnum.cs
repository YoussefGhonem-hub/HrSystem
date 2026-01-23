using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentCategoryEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_EmployeeDocumentTypes", x => x.Id);
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
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeDocuments_EmployeeDocumentTypes_DocumentTypeId",
                schema: "Employee",
                table: "EmployeeDocuments");

            migrationBuilder.DropTable(
                name: "EmployeeDocumentTypes",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "Loans",
                schema: "Payroll");

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
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
