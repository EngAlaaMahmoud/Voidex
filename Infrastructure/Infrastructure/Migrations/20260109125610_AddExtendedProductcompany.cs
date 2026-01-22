using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedProductcompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_ProductGroup_ProductGroupId",
                table: "Company");

            migrationBuilder.DropIndex(
                name: "IX_Company_ProductGroupId",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "Company");

            migrationBuilder.CreateTable(
                name: "ProductCompany",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompany", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCompanyProductGroup",
                columns: table => new
                {
                    ProductCompaniesId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ProductGroupsId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCompanyProductGroup", x => new { x.ProductCompaniesId, x.ProductGroupsId });
                    table.ForeignKey(
                        name: "FK_ProductCompanyProductGroup_ProductCompany_ProductCompaniesId",
                        column: x => x.ProductCompaniesId,
                        principalTable: "ProductCompany",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCompanyProductGroup_ProductGroup_ProductGroupsId",
                        column: x => x.ProductGroupsId,
                        principalTable: "ProductGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompany_Name",
                table: "ProductCompany",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompanyProductGroup_ProductGroupsId",
                table: "ProductCompanyProductGroup",
                column: "ProductGroupsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCompanyProductGroup");

            migrationBuilder.DropTable(
                name: "ProductCompany");

            migrationBuilder.AddColumn<string>(
                name: "ProductGroupId",
                table: "Company",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Company_ProductGroupId",
                table: "Company",
                column: "ProductGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_ProductGroup_ProductGroupId",
                table: "Company",
                column: "ProductGroupId",
                principalTable: "ProductGroup",
                principalColumn: "Id");
        }
    }
}
