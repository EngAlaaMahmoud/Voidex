using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedProductcompanyforProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCompanyId",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductCompanyId",
                table: "Product",
                column: "ProductCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_ProductCompany_ProductCompanyId",
                table: "Product",
                column: "ProductCompanyId",
                principalTable: "ProductCompany",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_ProductCompany_ProductCompanyId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_ProductCompanyId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ProductCompanyId",
                table: "Product");
        }
    }
}
