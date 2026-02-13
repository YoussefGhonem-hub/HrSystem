using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveRequireAttachmentToRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "BranchRequestSettings");

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "RequestTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "RequestTypes");

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "BranchRequestSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
