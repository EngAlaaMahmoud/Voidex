using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "PurchaseOrder",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "PurchaseOrder",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "VatAmount",
                table: "PurchaseOrder",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WithholdingAmount",
                table: "PurchaseOrder",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrder_TaxId",
                table: "PurchaseOrder",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrder_Tax_TaxId",
                table: "PurchaseOrder",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrder_Tax_TaxId",
                table: "PurchaseOrder");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrder_TaxId",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "PurchaseOrder");

            migrationBuilder.DropColumn(
                name: "WithholdingAmount",
                table: "PurchaseOrder");
        }
    }
}
