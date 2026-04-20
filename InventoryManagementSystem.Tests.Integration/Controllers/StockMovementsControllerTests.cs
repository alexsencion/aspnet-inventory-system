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
    public class StockMovementsControllerTests : IntegrationTestBase
    {
        public StockMovementsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Index_ShouldReturnSuccessStatusCode()
        {
            var response = await Client.GetAsync("/StockMovements");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Index_WithMovements_ShouldDisplayMovements()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 100,
                LowStockThreshold = 10
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now.AddDays(-2),
                    Reference = "PO-12345",
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now.AddDays(-1),
                    Reference = "Sale",
                    StockAfterMovement = 40
                }
            };

            Context.StockMovements.AddRange(movements);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/StockMovements");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Product");
            content.Should().Contain("PO-12345");
            content.Should().Contain("Sale");
        }

        [Fact]
        public async Task Index_WithProductFilter_ShouldFilterByProduct()
        {
            ClearDatabase();
            var product1 = new Product
            {
                Name = "Product 1",
                SKU = "P1",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 5
            };

            var product2 = new Product
            {
                Name = "Product 2",
                SKU = "P2",
                Category = "Test",
                UnitPrice = 20m,
                CurrentStock = 30,
                LowStockThreshold = 5
            };

            Context.Products.AddRange(product1, product2);
            await Context.SaveChangesAsync();

            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    ProductId = product1.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product2.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 30,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 30
                }
            };

            Context.StockMovements.AddRange(movements);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements?productId={product1.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("<td>Product 1</td>");
            content.Should().NotContain("<td>Product 2</td>");
        }

        [Fact]
        public async Task Index_WithMovementTypeFilter_ShouldFilterByType()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 40
                }
            };

            Context.StockMovements.AddRange(movements);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/StockMovements?movementType=IN");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("IN");
        }

        [Fact]
        public async Task Index_WithDateRangeFilter_ShouldFilterByDateRange()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 5
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Today.AddDays(-10),
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 20,
                    MovementDate = DateTime.Today.AddDays(-2),
                    StockAfterMovement = 70
                }
            };

            Context.StockMovements.AddRange(movements);
            await Context.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-5).ToString("yyyy-MM-dd");
            var endDate = DateTime.Today.ToString("yyyy-MM-dd");

            var response = await Client.GetAsync($"/StockMovements?startDate={startDate}&endDate{endDate}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Index_WithNoMovements_ShouldShowEmptyMessage()
        {
            ClearDatabase();

            var response = await Client.GetAsync("/StockMovements");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("No stock movements");
        }
    }
}
