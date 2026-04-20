using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class StockMovementsControllerCreateInTests : IntegrationTestBase
    {
        public StockMovementsControllerCreateInTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateIn_GET_ShouldReturnCreateForm()
        {
            var response = await Client.GetAsync("/StockMovements/CreateIn");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Record Stock IN");
            content.Should().Contain("Product");
            content.Should().Contain("Quantity");
            content.Should().Contain("Notes");
        }

        [Fact]
        public async Task CreateIn_GET_ShouldIncludeProductDropdown()
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

            var response = await Client.GetAsync("/StockMovements/CreateIn");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Product");
            content.Should().Contain("TEST-001");
        }

        [Fact]
        public async Task CreateIn_POST_WithValidData_ShouldCreateMovement()
        {
            // Arrange
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

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "30" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reference", "PO-12345" },
                { "Notes", "Test stock in" }
            };

            // Act
            var response = await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            Context.ChangeTracker.Clear();

            // Verify movement was created
            var movement = Context.StockMovements.FirstOrDefault(m => m.Reference == "PO-12345");
            movement.Should().NotBeNull();
            movement!.MovementType.Should().Be(MovementType.IN);
            movement.Quantity.Should().Be(30);

            // Verify product stock was updated
            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(80); // 50 + 30
        }

        [Fact]
        public async Task CreateIn_POST_ShouldUpdateStockAfterMovement()
        {
            // Arrange
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

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "25" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reference", "PO-TEST" }
            };

            // Act
            await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData));

            // Assert
            var movement = Context.StockMovements.FirstOrDefault(m => m.Reference == "PO-TEST");
            movement.Should().NotBeNull();
            movement!.StockAfterMovement.Should().Be(125); // 100 + 25
        }

        [Fact]
        public async Task CreateIn_POST_WithInvalidData_ShouldReturnValidationErrors()
        {
            var formData = new Dictionary<string, string>
            {
                { "ProductId", "" },
                { "Quantity", "0" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") }
            };

            var response = await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Record Stock IN");
        }

        [Fact]
        public async Task CreateIn_POST_WithFutureDate_ShouldShowError()
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

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "30" },
                { "MovementDate", DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm") },
                { "Reference", "PO-12345" }
            };

            var response = await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("cannot be in the future");
        }

        [Fact]
        public async Task CreateIn_POST_WithMultipleMovements_ShouldAccumulatedStock()
        {
            // Arrange
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

            // First movement
            var formData1 = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "30" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reference", "PO-001" }
            };
            await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData1));

            // Second movement
            var formData2 = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "20" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reference", "PO-002" }
            };
            await Client.PostAsync("/StockMovements/CreateIn", new FormUrlEncodedContent(formData2));

            // Assert
            Context.ChangeTracker.Clear();
            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(100); // 50 + 30 + 20
        }
    }
}
