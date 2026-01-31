using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgFullModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Employee",
                table: "JobTitles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "JobTitles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                schema: "Employee",
                table: "JobTitles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Employee",
                table: "JobTitles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Employee",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Employee",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                schema: "Employee",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "Employee",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill OrganizationId for existing data to avoid FK conflicts
            migrationBuilder.Sql(@"UPDATE Employee.Departments SET OrganizationId = TenantId WHERE OrganizationId = '00000000-0000-0000-0000-000000000000'");
            migrationBuilder.Sql(@"UPDATE Employee.JobTitles    SET OrganizationId = TenantId WHERE OrganizationId = '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateTable(
                name: "BranchHolidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurringMonth = table.Column<int>(type: "int", nullable: true),
                    RecurringDay = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BranchHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchHolidays_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchWorkSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    WorkingHoursPerDay = table.Column<int>(type: "int", nullable: false),
                    WorkingDaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    GracePeriodLate = table.Column<TimeSpan>(type: "time", nullable: true),
                    GracePeriodEarlyLeave = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsSunday = table.Column<bool>(type: "bit", nullable: false),
                    IsMonday = table.Column<bool>(type: "bit", nullable: false),
                    IsTuesday = table.Column<bool>(type: "bit", nullable: false),
                    IsWednesday = table.Column<bool>(type: "bit", nullable: false),
                    IsThursday = table.Column<bool>(type: "bit", nullable: false),
                    IsFriday = table.Column<bool>(type: "bit", nullable: false),
                    IsSaturday = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BranchWorkSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchWorkSchedules_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_OrganizationId",
                schema: "Employee",
                table: "JobTitles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId",
                schema: "Employee",
                table: "Departments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_BranchId",
                table: "BranchHolidays",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchHolidays_IsDeleted",
                table: "BranchHolidays",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkSchedules_BranchId",
                table: "BranchWorkSchedules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkSchedules_IsDeleted",
                table: "BranchWorkSchedules",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                schema: "Employee",
                table: "Departments",
                column: "OrganizationId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_Organizations_OrganizationId",
                schema: "Employee",
                table: "JobTitles",
                column: "OrganizationId",
                principalSchema: "Organization",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Organizations_OrganizationId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_Organizations_OrganizationId",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropTable(
                name: "BranchHolidays");

            migrationBuilder.DropTable(
                name: "BranchWorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_OrganizationId",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_Departments_OrganizationId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "Employee",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "Employee",
                table: "Departments");
        }
    }
}
