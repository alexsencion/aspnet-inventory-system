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
    public class StockMovementsControllerDetailsTests : IntegrationTestBase
    {
        public StockMovementsControllerDetailsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Details_WithValidId_ShouldReturnMovementDetails()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 50,
                MovementDate = DateTime.Now,
                Reference = "PO-12345",
                Notes = "Test stock IN",
                StockAfterMovement = 50
            };

            Context.StockMovements.Add(movement);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements/Details/{movement.MovementId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Product");
            content.Should().Contain("PO-12345");
            content.Should().Contain("Test stock IN");
            content.Should().Contain("50");
        }

        [Fact]
        public async Task Details_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await Client.GetAsync("/StockMovements/Details/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Details_WithStockINMovement_ShouldShowReference()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 5
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 50,
                MovementDate = DateTime.Now,
                Reference = "PURCHASE-ORDER-123",
                StockAfterMovement = 50
            };

            Context.StockMovements.Add(movement);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements/Details/{movement.MovementId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("PURCHASE-ORDER-123");
        }

        [Fact]
        public async Task Details_WithStockOUTMovement_ShouldShowReason()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 40,
                LowStockThreshold = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.OUT,
                Quantity = 10,
                MovementDate = DateTime.Now,
                Reason = "Damage",
                StockAfterMovement = 40
            };

            Context.StockMovements.Add(movement);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/StockMovements/Details/{movement.MovementId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Damage");
        }
    }
}
