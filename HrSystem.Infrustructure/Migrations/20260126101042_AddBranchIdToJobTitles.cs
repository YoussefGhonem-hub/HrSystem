using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToJobTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "Employee",
                table: "JobTitles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserBranchRoles",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_UserBranchRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBranchRoles_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBranchRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_BranchId",
                schema: "Employee",
                table: "JobTitles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBranchRoles_BranchId",
                schema: "security",
                table: "UserBranchRoles",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBranchRoles_UserId_BranchId_RoleName",
                schema: "security",
                table: "UserBranchRoles",
                columns: new[] { "UserId", "BranchId", "RoleName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_Branches_BranchId",
                schema: "Employee",
                table: "JobTitles",
                column: "BranchId",
                principalSchema: "Organization",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_Branches_BranchId",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropTable(
                name: "UserBranchRoles",
                schema: "security");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_BranchId",
                schema: "Employee",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Employee",
                table: "JobTitles");
        }
    }
}
