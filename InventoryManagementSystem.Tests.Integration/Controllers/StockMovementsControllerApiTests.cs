using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class StockMovementsControllerApiTests : IntegrationTestBase
    {
        public StockMovementsControllerApiTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetProductStock_WithValidProductId_ShouldReturnProductInfo()
        {
            ClearDatabase();
            var product = new Product 
            { 
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 75,
                LowStockThreshold = 20
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements/GetProductStock?productId={product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JsonDocument.Parse(content);
            json.RootElement.GetProperty("productName").GetString().Should().Be("Test Product");
            json.RootElement.GetProperty("sku").GetString().Should().Be("TEST-001");
            json.RootElement.GetProperty("currentStock").GetInt32().Should().Be(75);
            json.RootElement.GetProperty("lowStockThreshold").GetInt32().Should().Be(20);
            json.RootElement.GetProperty("isLowStock").GetBoolean().Should().BeFalse();
        }

        [Fact]
        public async Task GetProductStock_WithLowStockProduct_ShouldIndicateLowStock()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Low Stock Product",
                SKU = "LOW-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 5,
                LowStockThreshold = 20
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements/GetProductStock?productId={product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JsonDocument.Parse(content);
            json.RootElement.GetProperty("isLowStock").GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task GetProductStock_WithInvalidProductId_ShouldReturnNotFound()
        {
            var response = await Client.GetAsync("/StockMovements/GetProductStock?productId=999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
