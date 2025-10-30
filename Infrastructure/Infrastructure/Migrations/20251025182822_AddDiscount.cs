using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product");

            migrationBuilder.RenameColumn(
                name: "TaxAmount",
                table: "SalesOrder",
                newName: "WithholdingAmount");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "SalesOrder",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "SalesOrder",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VatAmount",
                table: "SalesOrder",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrder_TaxId",
                table: "SalesOrder",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrder_Tax_TaxId",
                table: "SalesOrder",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrder_Tax_TaxId",
                table: "SalesOrder");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrder_TaxId",
                table: "SalesOrder");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "SalesOrder");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "SalesOrder");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "SalesOrder");

            migrationBuilder.RenameColumn(
                name: "WithholdingAmount",
                table: "SalesOrder",
                newName: "TaxAmount");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
