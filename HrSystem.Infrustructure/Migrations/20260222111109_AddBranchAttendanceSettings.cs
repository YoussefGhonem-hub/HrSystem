using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchAttendanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CheckInLatitude",
                schema: "Attendance",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckInLongitude",
                schema: "Attendance",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInMethod",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckInPointId",
                schema: "Attendance",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLatitude",
                schema: "Attendance",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CheckOutLongitude",
                schema: "Attendance",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckOutMethod",
                schema: "Attendance",
                table: "Attendances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckOutPointId",
                schema: "Attendance",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchAttendanceSettings",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AllowFaceId = table.Column<bool>(type: "bit", nullable: false),
                    AllowLocation = table.Column<bool>(type: "bit", nullable: false),
                    AllowExcelImport = table.Column<bool>(type: "bit", nullable: false),
                    AllowFingerprint = table.Column<bool>(type: "bit", nullable: false),
                    AllowManual = table.Column<bool>(type: "bit", nullable: false),
                    RequireLocationValidation = table.Column<bool>(type: "bit", nullable: false),
                    DefaultGeofenceRadiusMeters = table.Column<int>(type: "int", nullable: false, defaultValue: 200),
                    AutoCheckoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AutoCheckoutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    FaceIdConfidenceThreshold = table.Column<double>(type: "float", nullable: false, defaultValue: 0.84999999999999998),
                    FaceIdRequireLiveness = table.Column<bool>(type: "bit", nullable: false),
                    ExcelImportSkipDuplicates = table.Column<bool>(type: "bit", nullable: false),
                    ExcelDateFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AllowMultipleCheckInsPerDay = table.Column<bool>(type: "bit", nullable: false),
                    MinCheckInDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_BranchAttendanceSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchAttendanceSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchCheckInPoints",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchAttendanceSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    RadiusMeters = table.Column<int>(type: "int", nullable: true),
                    IsCheckInPoint = table.Column<bool>(type: "bit", nullable: false),
                    IsCheckOutPoint = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BranchCheckInPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchCheckInPoints_BranchAttendanceSettings_BranchAttendanceSettingId",
                        column: x => x.BranchAttendanceSettingId,
                        principalSchema: "Attendance",
                        principalTable: "BranchAttendanceSettings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BranchCheckInPoints_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CheckInPointId",
                schema: "Attendance",
                table: "Attendances",
                column: "CheckInPointId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_CheckOutPointId",
                schema: "Attendance",
                table: "Attendances",
                column: "CheckOutPointId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAttendanceSettings_BranchId",
                schema: "Attendance",
                table: "BranchAttendanceSettings",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchAttendanceSettings_IsDeleted",
                schema: "Attendance",
                table: "BranchAttendanceSettings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_BranchCheckInPoints_BranchAttendanceSettingId",
                schema: "Attendance",
                table: "BranchCheckInPoints",
                column: "BranchAttendanceSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchCheckInPoints_BranchId",
                schema: "Attendance",
                table: "BranchCheckInPoints",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchCheckInPoints_IsDeleted",
                schema: "Attendance",
                table: "BranchCheckInPoints",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_BranchCheckInPoints_CheckInPointId",
                schema: "Attendance",
                table: "Attendances",
                column: "CheckInPointId",
                principalSchema: "Attendance",
                principalTable: "BranchCheckInPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_BranchCheckInPoints_CheckOutPointId",
                schema: "Attendance",
                table: "Attendances",
                column: "CheckOutPointId",
                principalSchema: "Attendance",
                principalTable: "BranchCheckInPoints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_BranchCheckInPoints_CheckInPointId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_BranchCheckInPoints_CheckOutPointId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "BranchCheckInPoints",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "BranchAttendanceSettings",
                schema: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_CheckInPointId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_CheckOutPointId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLatitude",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInLongitude",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInMethod",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckInPointId",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLatitude",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutLongitude",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutMethod",
                schema: "Attendance",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "CheckOutPointId",
                schema: "Attendance",
                table: "Attendances");
        }
    }
}
