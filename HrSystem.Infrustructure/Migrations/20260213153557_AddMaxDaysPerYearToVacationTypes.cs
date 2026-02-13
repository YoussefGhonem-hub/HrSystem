using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxDaysPerYearToVacationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxDaysPerYear",
                schema: "Requests",
                table: "VacationTypes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxDaysPerYear",
                schema: "Requests",
                table: "VacationTypes");
        }
    }
}
