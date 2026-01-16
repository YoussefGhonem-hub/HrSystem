using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class branches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Employee",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Employee",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

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
                    Country = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Employees_BranchManagerId",
                        column: x => x.BranchManagerId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Branches_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BranchId",
                schema: "Employee",
                table: "Employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BranchId",
                schema: "Employee",
                table: "Departments",
                column: "BranchId");

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
                name: "IX_Branches_Country",
                schema: "Organization",
                table: "Branches",
                column: "Country");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Branches_BranchId",
                schema: "Employee",
                table: "Departments",
                column: "BranchId",
                principalSchema: "Organization",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Branches_BranchId",
                schema: "Employee",
                table: "Employees",
                column: "BranchId",
                principalSchema: "Organization",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Branches_BranchId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Branches_BranchId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_Employees_BranchId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Departments_BranchId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Employee",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Employee",
                table: "Departments");
        }
    }
}
