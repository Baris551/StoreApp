using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => new { x.CategoryId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_ProductCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategory_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Url" },
                values: new object[,]
                {
                    { 1, "Telefon", "telefon" },
                    { 2, "Elektronik", "elektronik" },
                    { 3, "Beyaz Eşya", "beyaz-esya" }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "İphone 1", "İphone 1" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name" },
                values: new object[] { "İphone 2", "İphone 2" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price" },
                values: new object[] { "MacOs ", "MacOs", 50m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Çamaşır Makinesi", "Çamaşır Makinesi" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Honor PC", "Honor PC" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "İphone 6", "İphone 6" });

            migrationBuilder.InsertData(
                table: "ProductCategory",
                columns: new[] { "CategoryId", "ProductId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 6 },
                    { 2, 3 },
                    { 2, 5 },
                    { 3, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Url",
                table: "Categories",
                column: "Url",
                unique: true,
                filter: "[Url] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_ProductId",
                table: "ProductCategory",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Telefon", "Description 1", "Product 1" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Bilgisayar", "Description 2", "Product 2" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Description", "Name", "Price" },
                values: new object[] { "Elektronik", "Elektronik 3", "Product 3", 3000m });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Telefon", "Description 4", "Product 4" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Bilgisayar", "Description 5", "Product 5" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Elektronik", "Elektronik 6", "Product 6" });
        }
    }
}
