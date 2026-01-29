using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HandleTaxesForProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MainCode",
                table: "ProductTaxes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "ProductTaxes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "SubCode",
                table: "ProductTaxes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TaxValue",
                table: "ProductTaxes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MainCode",
                table: "ProductTaxes");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "ProductTaxes");

            migrationBuilder.DropColumn(
                name: "SubCode",
                table: "ProductTaxes");

            migrationBuilder.DropColumn(
                name: "TaxValue",
                table: "ProductTaxes");
        }
    }
}
