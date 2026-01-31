using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryPayrollMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranch",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankIban",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankSwiftCode",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSocialInsuranceEnabled",
                schema: "Payroll",
                table: "Salaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "Payroll",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SocialInsuranceEmployeeRate",
                schema: "Payroll",
                table: "Salaries",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SocialInsuranceEmployerRate",
                schema: "Payroll",
                table: "Salaries",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "BankBranch",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "BankIban",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "BankName",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "BankSwiftCode",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "IsSocialInsuranceEnabled",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "SocialInsuranceEmployeeRate",
                schema: "Payroll",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "SocialInsuranceEmployerRate",
                schema: "Payroll",
                table: "Salaries");
        }
    }
}
