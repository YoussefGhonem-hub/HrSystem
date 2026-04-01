using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBankExportProfileTemplateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateFileKey",
                schema: "Payroll",
                table: "BankExportProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemplateFileName",
                schema: "Payroll",
                table: "BankExportProfiles",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateFileKey",
                schema: "Payroll",
                table: "BankExportProfiles");

            migrationBuilder.DropColumn(
                name: "TemplateFileName",
                schema: "Payroll",
                table: "BankExportProfiles");
        }
    }
}
