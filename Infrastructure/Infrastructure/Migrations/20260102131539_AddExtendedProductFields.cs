using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AdditionalFee",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AdditionalTax",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Product",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GisEgsCode",
                table: "Product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalCode",
                table: "Product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Product",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PriceAfterDiscount",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ServiceFee",
                table: "Product",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_InternalCode",
                table: "Product",
                column: "InternalCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Product_InternalCode",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "AdditionalFee",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "AdditionalTax",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "GisEgsCode",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "InternalCode",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "PriceAfterDiscount",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ServiceFee",
                table: "Product");
        }
    }
}
