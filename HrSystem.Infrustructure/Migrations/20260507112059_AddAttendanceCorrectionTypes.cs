using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceCorrectionTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceCorrectionTypes",
                schema: "Requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresManagerApproval = table.Column<bool>(type: "bit", nullable: false),
                    RequireAttachment = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AttendanceCorrectionTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCorrectionTypes_IsDeleted",
                schema: "Requests",
                table: "AttendanceCorrectionTypes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceCorrectionTypes_NameEn",
                schema: "Requests",
                table: "AttendanceCorrectionTypes",
                column: "NameEn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceCorrectionTypes",
                schema: "Requests");
        }
    }
}
