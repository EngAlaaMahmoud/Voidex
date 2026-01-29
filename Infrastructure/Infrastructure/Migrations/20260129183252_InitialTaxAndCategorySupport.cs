using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTaxAndCategorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_TaxId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Product");

            migrationBuilder.AddColumn<string>(
                name: "TaxCategoryId",
                table: "Tax",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductTax",
                columns: table => new
                {
                    ProductsId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TaxesId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTax", x => new { x.ProductsId, x.TaxesId });
                    table.ForeignKey(
                        name: "FK_ProductTax_Product_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTax_Tax_TaxesId",
                        column: x => x.TaxesId,
                        principalTable: "Tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTaxes",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true),
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTaxes", x => new { x.ProductId, x.TaxId });
                    table.ForeignKey(
                        name: "FK_ProductTaxes_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTaxes_Tax_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tax_TaxCategoryId",
                table: "Tax",
                column: "TaxCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTax_TaxesId",
                table: "ProductTax",
                column: "TaxesId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTaxes_TaxId",
                table: "ProductTaxes",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCategory_Code",
                table: "TaxCategory",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tax_TaxCategory_TaxCategoryId",
                table: "Tax",
                column: "TaxCategoryId",
                principalTable: "TaxCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tax_TaxCategory_TaxCategoryId",
                table: "Tax");

            migrationBuilder.DropTable(
                name: "ProductTax");

            migrationBuilder.DropTable(
                name: "ProductTaxes");

            migrationBuilder.DropTable(
                name: "TaxCategory");

            migrationBuilder.DropIndex(
                name: "IX_Tax_TaxCategoryId",
                table: "Tax");

            migrationBuilder.DropColumn(
                name: "TaxCategoryId",
                table: "Tax");

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Product",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_TaxId",
                table: "Product",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Tax_TaxId",
                table: "Product",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id");
        }
    }
}
