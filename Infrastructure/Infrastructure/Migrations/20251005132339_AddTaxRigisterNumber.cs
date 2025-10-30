using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRigisterNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FaxNumber",
                table: "Vendor",
                newName: "TaxRegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "FaxNumber",
                table: "Customer",
                newName: "TaxRegistrationNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxRegistrationNumber",
                table: "Vendor",
                newName: "FaxNumber");

            migrationBuilder.RenameColumn(
                name: "TaxRegistrationNumber",
                table: "Customer",
                newName: "FaxNumber");
        }
    }
}
