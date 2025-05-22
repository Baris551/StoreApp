using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddElektronicCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Elektronik 3");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Elektronik 6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Description 3");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Description 6");
        }
    }
}
