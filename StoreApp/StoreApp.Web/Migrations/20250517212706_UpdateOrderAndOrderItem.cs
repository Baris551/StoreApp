using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItem",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "OrderItem",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "OrderItem");

            migrationBuilder.AlterColumn<double>(
                name: "Price",
                table: "OrderItem",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
