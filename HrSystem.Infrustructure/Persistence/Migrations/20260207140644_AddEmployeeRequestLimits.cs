using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeRequestLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeePermissionLimits",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxHoursPerMonth = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmployeePermissionLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePermissionLimits_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePermissionLimits_PermissionTypes_PermissionTypeId",
                        column: x => x.PermissionTypeId,
                        principalSchema: "Requests",
                        principalTable: "PermissionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeVacationLimits",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxDaysPerYear = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmployeeVacationLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeVacationLimits_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeVacationLimits_VacationTypes_VacationTypeId",
                        column: x => x.VacationTypeId,
                        principalSchema: "Requests",
                        principalTable: "VacationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePermissionLimits_EmployeeId_PermissionTypeId",
                schema: "Requests",
                table: "EmployeePermissionLimits",
                columns: new[] { "EmployeeId", "PermissionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePermissionLimits_IsDeleted",
                schema: "Requests",
                table: "EmployeePermissionLimits",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePermissionLimits_PermissionTypeId",
                schema: "Requests",
                table: "EmployeePermissionLimits",
                column: "PermissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeVacationLimits_EmployeeId_VacationTypeId",
                schema: "Requests",
                table: "EmployeeVacationLimits",
                columns: new[] { "EmployeeId", "VacationTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeVacationLimits_IsDeleted",
                schema: "Requests",
                table: "EmployeeVacationLimits",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeVacationLimits_VacationTypeId",
                schema: "Requests",
                table: "EmployeeVacationLimits",
                column: "VacationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePermissionLimits",
                schema: "Requests");

            migrationBuilder.DropTable(
                name: "EmployeeVacationLimits",
                schema: "Requests");
        }
    }
}
