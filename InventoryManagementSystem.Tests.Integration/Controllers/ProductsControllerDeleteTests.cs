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
    public class ProductsControllerDeleteTests : IntegrationTestBase
    {
        public ProductsControllerDeleteTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnDeleteConfirmation()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product To Delete",
                SKU = "DEL-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 25,
                LowStockThreshold = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Delete/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Delete Product");
            content.Should().Contain("Product To Delete");
            content.Should().Contain("DEL-001");
        }

        [Fact]
        public async Task Delete_GET_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await Client.GetAsync("Products/Delete/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_GET_WithProductHavingStockMovements_ShouldShowWarning()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product With Movements",
                SKU = "PWM-001",
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
                StockAfterMovement = 50
            };

            Context.StockMovements.Add(movement);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Delete/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("cannot be deleted");
            content.Should().Contain("stock movement history");
        }

        [Fact]
        public async Task Delete_POST_WithValidIdAndNoMovements_ShouldDeleteProduct()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product To Delete",
                SKU = "DEL-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 0,
                LowStockThreshold = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();
            var productId = product.ProductId;

            var formData = new Dictionary<string, string>
            {
                { "ProductId", productId.ToString() }
            };

            var response = await Client.PostAsync($"/Products/Delete/{productId}", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            Context.ChangeTracker.Clear();

            var deletedProduct = await Context.Products.FindAsync(productId);
            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task Delete_POST_WithProductHavingStockMovements_ShouldNotDelete()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product With Movements",
                SKU = "PWM-001",
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
                StockAfterMovement = 50
            };

            Context.StockMovements.Add(movement);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() }
            };

            var response = await Client.PostAsync($"/Products/Delete/{product.ProductId}", new FormUrlEncodedContent(formData));

            var existingProduct = await Context.Products.FindAsync(product.ProductId);
            existingProduct.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_POST_WithInvalidId_ShouldReturnRedirect()
        {
            var formData = new Dictionary<string, string>
            {
                { "ProductId", "999" }
            };

            var response = await Client.PostAsync($"/Products/Delete/999", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found, HttpStatusCode.OK);
        }

        [Fact]
        public async Task Delete_ShouldNotAffectSupplier()
        {
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John",
                Email = "john@supplier.com",
                Phone = "111"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Product",
                SKU = "PROD-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 5,
                SupplierId = supplier.SupplierId
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() }
            };

            var response = await Client.PostAsync($"/Products/Delete/{product.ProductId}", new FormUrlEncodedContent(formData));

            Context.ChangeTracker.Clear();

            var existingSupplier = await Context.Suppliers.FindAsync(supplier.SupplierId);
            existingSupplier.Should().NotBeNull();
        }
    }
}
