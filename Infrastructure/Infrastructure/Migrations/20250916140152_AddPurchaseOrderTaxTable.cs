using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderTaxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustomerCategory_CustomerCategoryId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_CustomerGroup_CustomerGroupId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrder_Tax_TaxId",
                table: "SalesOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_VendorCategory_VendorCategoryId",
                table: "Vendor");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_VendorGroup_VendorGroupId",
                table: "Vendor");

            migrationBuilder.DropTable(
                name: "CustomerCategory");

            migrationBuilder.DropIndex(
                name: "IX_Vendor_VendorCategoryId",
                table: "Vendor");

            migrationBuilder.DropIndex(
                name: "IX_Vendor_VendorGroupId",
                table: "Vendor");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrder_TaxId",
                table: "SalesOrder");

            migrationBuilder.DropIndex(
                name: "IX_CustomerGroup_Name",
                table: "CustomerGroup");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CustomerCategoryId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CustomerGroupId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "LinkedIn",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "TikTok",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "TwitterX",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "VendorCategoryId",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "VendorGroupId",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "SalesOrder");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CustomerCategoryId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CustomerGroupId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LinkedIn",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TikTok",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TwitterX",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "WhatsApp",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerGroup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CustomerGroup",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PurchaseOrderTaxes",
                columns: table => new
                {
                    PurchaseOrderId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderTaxes", x => new { x.PurchaseOrderId, x.TaxId });
                    table.ForeignKey(
                        name: "FK_PurchaseOrderTaxes_PurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderTaxes_Tax_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderTaxes",
                columns: table => new
                {
                    SalesOrderId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderTaxes", x => new { x.SalesOrderId, x.TaxId });
                    table.ForeignKey(
                        name: "FK_SalesOrderTaxes_SalesOrder_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalTable: "SalesOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesOrderTaxes_Tax_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Tax",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderTaxes_TaxId",
                table: "PurchaseOrderTaxes",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderTaxes_TaxId",
                table: "SalesOrderTaxes",
                column: "TaxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderTaxes");

            migrationBuilder.DropTable(
                name: "SalesOrderTaxes");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedIn",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TikTok",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterX",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorCategoryId",
                table: "Vendor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorGroupId",
                table: "Vendor",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Vendor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "SalesOrder",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomerGroup",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CustomerGroup",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerCategoryId",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerGroupId",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedIn",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TikTok",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterX",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsApp",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_VendorCategoryId",
                table: "Vendor",
                column: "VendorCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_VendorGroupId",
                table: "Vendor",
                column: "VendorGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrder_TaxId",
                table: "SalesOrder",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerGroup_Name",
                table: "CustomerGroup",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustomerCategoryId",
                table: "Customer",
                column: "CustomerCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustomerGroupId",
                table: "Customer",
                column: "CustomerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCategory_Name",
                table: "CustomerCategory",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustomerCategory_CustomerCategoryId",
                table: "Customer",
                column: "CustomerCategoryId",
                principalTable: "CustomerCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_CustomerGroup_CustomerGroupId",
                table: "Customer",
                column: "CustomerGroupId",
                principalTable: "CustomerGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrder_Tax_TaxId",
                table: "SalesOrder",
                column: "TaxId",
                principalTable: "Tax",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_VendorCategory_VendorCategoryId",
                table: "Vendor",
                column: "VendorCategoryId",
                principalTable: "VendorCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_VendorGroup_VendorGroupId",
                table: "Vendor",
                column: "VendorGroupId",
                principalTable: "VendorGroup",
                principalColumn: "Id");
        }
    }
}
