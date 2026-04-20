using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class StockMovementsControllerCreateOutTests : IntegrationTestBase
    {
        public StockMovementsControllerCreateOutTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateOut_GET_ShouldReturnCreateForm()
        {
            var response = await Client.GetAsync("/StockMovements/CreateOut");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Record Stock OUT");
            content.Should().Contain("Product");
            content.Should().Contain("Quantity");
            content.Should().Contain("Reason");
        }

        [Fact]
        public async Task CreateOut_POST_WithValidData_ShouldCreateMovement()
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
                { "Quantity", "20" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "Sale" },
                { "Notes", "Test sale" },
            };

            var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));

            Context.ChangeTracker.Clear();

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            var movement = Context.StockMovements.FirstOrDefault(m => m.Reason == "Sale");
            movement.Should().NotBeNull();
            movement!.MovementType.Should().Be(MovementType.OUT);
            movement.Quantity.Should().Be(20);

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(30);
        }

        [Fact]
        public async Task CreateOut_POST_WithInsufficientStock_ShouldShowError()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 10,
                LowStockThreshold = 5,
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "20" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "Sale" },
            };

            var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Insufficient stock");

            var unchangedProduct = await Context.Products.FindAsync(product.ProductId);
            unchangedProduct!.CurrentStock.Should().Be(10);

            var movements = Context.StockMovements.Where(m => m.ProductId == product.ProductId);
            movements.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateOut_POST_ToZeroStock_ShouldSucceed()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 25,
                LowStockThreshold = 10
            };
            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Quantity", "25" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "Sale" },
            };

            var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));

            Context.ChangeTracker.Clear();

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(0);
        }

        [Fact]
        public async Task CreateOut_POST_WithInvalidData_ShouldReturnValidationErrors()
        {
            var formData = new Dictionary<string, string>
            {
                { "ProductId", "" },
                { "Quantity", "0" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "" }
            };

            var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Record Stock OUT");
        }

        [Fact]
        public async Task CreateOut_POST_WithFutureDate_ShouldShowError()
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
                { "Quantity", "10" },
                { "MovementDate", DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "Sale" }
            };

            var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("cannot be in the future");
        }

        [Fact]
        public async Task CreateOut_POST_ShouldUpdateStockAfterMovement()
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
                { "Quantity", "35" },
                { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                { "Reason", "Damage" }
            };

            // Act
            await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));

            // Assert
            var movement = Context.StockMovements.FirstOrDefault(m => m.Reason == "Damage");
            movement.Should().NotBeNull();
            movement!.StockAfterMovement.Should().Be(65); // 100 - 35
        }

        [Fact]
        public async Task CreateOut_POST_WithMultipleReasons_ShouldAcceptAllReasons()
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

            var reasons = new[] { "Sale", "Damage", "Loss", "Return", "Other" };

            foreach (var reason in reasons)
            {
                var formData = new Dictionary<string, string>
                {
                    { "ProductId", product.ProductId.ToString() },
                    { "Quantity", "5" },
                    { "MovementDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm") },
                    { "Reason", reason }
                };

                // Act
                var response = await Client.PostAsync("/StockMovements/CreateOut", new FormUrlEncodedContent(formData));

                // Assert
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);
            }

            Context.ChangeTracker.Clear();

            // Verify all movements were created
            var movements = Context.StockMovements.Where(m => m.ProductId == product.ProductId).ToList();
            movements.Should().HaveCount(5);
            movements.Select(m => m.Reason).Should().BeEquivalentTo(reasons);

            // Verify final stock
            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(75); // 100 - (5 * 5)
        }
    }
}
