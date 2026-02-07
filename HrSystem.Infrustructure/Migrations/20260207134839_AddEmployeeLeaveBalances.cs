using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLeaveBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Leave");

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveBalances",
                schema: "Leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    AllocatedDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    CarryOverDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    ManualAdjustmentDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
                    UsedDays = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_EmployeeLeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_VacationTypes_VacationTypeId",
                        column: x => x.VacationTypeId,
                        principalSchema: "Requests",
                        principalTable: "VacationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveTransactions",
                schema: "Leave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeLeaveBalanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacationTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    DaysChanged = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_EmployeeLeaveTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveTransactions_EmployeeLeaveBalances_EmployeeLeaveBalanceId",
                        column: x => x.EmployeeLeaveBalanceId,
                        principalSchema: "Leave",
                        principalTable: "EmployeeLeaveBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId_VacationTypeId_Year",
                schema: "Leave",
                table: "EmployeeLeaveBalances",
                columns: new[] { "EmployeeId", "VacationTypeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_IsDeleted",
                schema: "Leave",
                table: "EmployeeLeaveBalances",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_VacationTypeId",
                schema: "Leave",
                table: "EmployeeLeaveBalances",
                column: "VacationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveTransactions_EmployeeId_VacationTypeId_Year",
                schema: "Leave",
                table: "EmployeeLeaveTransactions",
                columns: new[] { "EmployeeId", "VacationTypeId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveTransactions_EmployeeLeaveBalanceId",
                schema: "Leave",
                table: "EmployeeLeaveTransactions",
                column: "EmployeeLeaveBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveTransactions_IsDeleted",
                schema: "Leave",
                table: "EmployeeLeaveTransactions",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLeaveTransactions",
                schema: "Leave");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveBalances",
                schema: "Leave");
        }
    }
}
