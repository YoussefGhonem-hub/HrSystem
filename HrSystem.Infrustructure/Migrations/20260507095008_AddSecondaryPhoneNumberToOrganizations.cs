using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondaryPhoneNumberToOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Organization.Organizations', 'SecondaryPhoneNumber') IS NULL
BEGIN
    ALTER TABLE [Organization].[Organizations]
    ADD [SecondaryPhoneNumber] nvarchar(20) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Organization.Organizations', 'SecondaryPhoneNumber') IS NOT NULL
BEGIN
    ALTER TABLE [Organization].[Organizations]
    DROP COLUMN [SecondaryPhoneNumber];
END
");
        }
    }
}
