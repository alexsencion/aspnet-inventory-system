using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class ProductsControllerTests : IntegrationTestBase
    {
        public ProductsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Index_ShouldReturnSuccessStatusCode()
        {
            var response = await Client.GetAsync("/Products");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Index_WithProducts_ShouldDisplayProducts()
        {
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "john@supplier.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Wireless Mouse",
                    SKU = "WM-001",
                    Category = "Accessories",
                    UnitPrice = 29.99m,
                    CurrentStock = 50,
                    LowStockThreshold = 10,
                    SupplierId = supplier.SupplierId
                },

                new Product
                {
                    Name = "Mechanical Keyboard",
                    SKU = "KB-002",
                    Category = "Accessories",
                    UnitPrice = 89.99m,
                    CurrentStock = 30,
                    LowStockThreshold = 5,
                    SupplierId = supplier.SupplierId
                },
            };

            Context.Products.AddRange(products);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Products");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Wireless Mouse");
            content.Should().Contain("Mechanical Keyboard");
            content.Should().Contain("WM-001");
            content.Should().Contain("KB-002");
        }

        [Fact]
        public async Task Index_WithSearchTerm_ShouldFilterResults()
        {
            ClearDatabase();
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Wireless Mouse",
                    SKU = "WM-001",
                    Category = "Accessories",
                    UnitPrice = 29.99m,
                    CurrentStock = 50,
                    LowStockThreshold = 10
                },
                new Product
                {
                    Name = "USB-C Cable",
                    SKU = "CBL-001",
                    Category = "Cables",
                    UnitPrice = 12.99m,
                    CurrentStock = 100,
                    LowStockThreshold = 20
                },
            };

            Context.Products.AddRange(products);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Products?searchTerm=Mouse");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Wireless Mouse");
            content.Should().NotContain("USB-C Cable");
        }

        [Fact]
        public async Task Index_WithCategoryFilter_ShouldFilterByCategory()
        {
            ClearDatabase();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Product A",
                    SKU = "PA-001",
                    Category = "Electronics",
                    UnitPrice = 50.00m,
                    CurrentStock = 25,
                    LowStockThreshold = 5
                },
                new Product
                {
                    Name = "Product B",
                    SKU = "PA-001",
                    Category = "Office",
                    UnitPrice = 20.00m,
                    CurrentStock = 40,
                    LowStockThreshold = 10
                },
            };

            Context.Products.AddRange(products);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Products?category=Electronics");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Product A");
            content.Should().NotContain("Product B");
        }

        [Fact]
        public async Task Index_WithLowStockFilter_ShouldShowOnlyLowStockItems()
        {
            ClearDatabase();
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Low Stock Item",
                    SKU = "LOW-001",
                    Category = "Test",
                    UnitPrice = 10.00m,
                    CurrentStock = 3,
                    LowStockThreshold = 10
                },
                new Product
                {
                    Name = "Normal Stock Item",
                    SKU = "NRM-001",
                    Category = "Test",
                    UnitPrice = 15.00m,
                    CurrentStock = 50,
                    LowStockThreshold = 10
                },
            };

            Context.Products.AddRange(products);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Products?lowStockOnly=true");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Low Stock Item");
            content.Should().NotContain("Normal Stock Item");
        }

        [Fact]
        public async Task Index_WithNoProducts_ShouldShowEmptyMessage()
        {
            ClearDatabase();

            var response = await Client.GetAsync("/Products");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("No products found");
        }

        [Fact]
        public async Task Index_WithLowStockProduct_ShouldHighlight()
        {
            ClearDatabase();

            var product = new Product
            {
                Name = "Low Stock Product",
                SKU = "LOW-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 2,
                LowStockThreshold = 10
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Products");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Low Stock Product");
            content.Should().Contain("table-warning");
        }
    }
}
