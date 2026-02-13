using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequireAttachmentToAllSubTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "VacationTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "TrainingTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "PersonalTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "PermissionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "OvertimeTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "MiscellaneousTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireAttachment",
                schema: "Requests",
                table: "FeedbackTypes",
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
                table: "VacationTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "TrainingTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "PersonalTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "PermissionTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "OvertimeTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "MiscellaneousTypes");

            migrationBuilder.DropColumn(
                name: "RequireAttachment",
                schema: "Requests",
                table: "FeedbackTypes");
        }
    }
}
