using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryManagementSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    MovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StockAfterMovement = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "SupplierId", "Address", "ContactPerson", "CreatedDate", "Email", "ModifiedDate", "Name", "Notes", "Phone" },
                values: new object[,]
                {
                    { 1, "123 Tech Ave, Silicon Valley, CA 94025", "John Smith", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2280), "john.smith@techdist.com", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2280), "Tech Distributors Inc.", null, "+1-555-0101" },
                    { 2, "456 Business Blvd, New York, NY 10001", "Sarah Johnson", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2284), "sarah.j@officesupplies.com", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2285), "Office Supplies Co.", null, "+1-555-0102" },
                    { 3, "789 Electronics Way, Austin, TX 78701", "Michael Chen", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2288), "m.chen@globalelec.com", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2288), "Global Electronics Ltd.", null, "+1-555-0103" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Category", "CreatedDate", "CurrentStock", "Description", "LowStockThreshold", "ModifiedDate", "Name", "SKU", "SupplierId", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "Computer Accessories", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2424), 50, "Ergonomic wireless mouse with USB receiver", 10, new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2425), "Wireless Mouse", "WM-001", 1, 29.99m },
                    { 2, "Computer Accessories", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2429), 30, "RGB mechanical gaming keyboard", 5, new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2430), "Mechanical Keyboard", "KB-002", 1, 89.99m },
                    { 3, "Cables", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2434), 100, "6ft USB-C to USB-C cable", 20, new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2434), "USB-C Cable", "CBL-003", 3, 12.99m },
                    { 4, "Office Supplies", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2438), 5, "500 sheets, 80gsm white paper", 15, new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2439), "A4 Printer Paper", "PPR-004", 2, 8.99m },
                    { 5, "Office Supplies", new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2442), 25, "Blue ink ballpoint pens", 10, new DateTime(2026, 2, 10, 3, 59, 38, 826, DateTimeKind.Local).AddTicks(2442), "Ballpoint Pens (Box of 12)", "PEN-005", 2, 5.99m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId",
                table: "Products",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_MovementDate",
                table: "StockMovements",
                column: "MovementDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_MovementType",
                table: "StockMovements",
                column: "MovementType");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
