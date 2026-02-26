using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterUniqueIndexForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "BranchId", "RequestTypeId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.CreateIndex(
                name: "IX_BranchRequestSettings_BranchId_RequestTypeId",
                schema: "Requests",
                table: "BranchRequestSettings",
                columns: new[] { "BranchId", "RequestTypeId" },
                unique: true);
        }
    }
}
