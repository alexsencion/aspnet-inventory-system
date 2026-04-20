using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class ProductsControllerEditTests : IntegrationTestBase
    {
        public ProductsControllerEditTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Edit_GET_WithValidId_ShouldReturnEditForm()
        {
            // Arrange
            ClearDatabase();
            var product = new Product
            {
                Name = "Original Product",
                SKU = "ORIG-001",
                Category = "Original Category",
                UnitPrice = 25.00m,
                CurrentStock = 50,
                LowStockThreshold = 10,
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/Products/Edit/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Edit Product");
            content.Should().Contain("Original Product");
            content.Should().Contain("ORIG-001");
        }

        [Fact]
        public async Task Edit_GET_WithInValidId_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.GetAsync("Products/Edit/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_POST_WithValidData_ShouldUpdateProduct()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Original Product",
                SKU = "ORIG-001",
                Category = "Original Category",
                UnitPrice = 25.00m,
                CurrentStock = 50,
                LowStockThreshold = 10,
                CreatedDate = DateTime.Now,
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Name", "Updated Product" },
                { "SKU", "UPD-001" },
                { "Description", "Updated description" },
                { "Category", "Updated Category" },
                { "UnitPrice", "35.00" },
                { "LowStockThreshold", "20" }
            };

            var response = await Client.PostAsync($"Products/Edit/{product.ProductId}", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            Context.ChangeTracker.Clear();

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct.Should().NotBeNull();
            updatedProduct!.Name.Should().Be("Updated Product");
            updatedProduct!.SKU.Should().Be("UPD-001");
            updatedProduct.UnitPrice.Should().Be(35.00m);
            updatedProduct.LowStockThreshold.Should().Be(20);
            updatedProduct.CurrentStock.Should().Be(50);
        }

        [Fact]
        public async Task Edit_POST_ShouldNotChangeCurrentStock()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product",
                SKU = "PROD-001",
                Category = "Category",
                UnitPrice = 10.00m,
                CurrentStock = 100,
                LowStockThreshold = 10
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Name", "Updated Product" },
                { "SKU", "PROD-001" },
                { "Category", "Category" },
                { "UnitPrice", "15.00" },
                { "LowStockThreshold", "15" }
            };

            var response = await Client.PostAsync($"/Products/Edit/{product.ProductId}", new FormUrlEncodedContent(formData));

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(100);
        }

        [Fact]
        public async Task Edit_POST_WithDuplicateSKU_ShouldShownError()
        {
            ClearDatabase();
            var product1 = new Product
            {
                Name = "Product 1",
                SKU = "PROD-001",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            var product2 = new Product
            {
                Name = "Product 2",
                SKU = "PROD-002",
                Category = "Test",
                UnitPrice = 20.00m,
                LowStockThreshold = 10
            };

            Context.Products.AddRange(product1, product2);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product2.ProductId.ToString() },
                { "Name", "Product 2" },
                { "SKU", "PROD-001" },
                { "Category", "Test" },
                { "UnitPrice", "20.00" },
                { "LowStockThreshold", "10" }
            };

            var response = await Client.PostAsync($"/Products/Edit/{product2.ProductId}", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("already exists");
        }

        [Fact]
        public async Task Edit_POST_WithSameSKU_ShouldSucceed()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product",
                SKU = "PROD-001",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Name", "Updated Product" },
                { "SKU", "PROD-001" },
                { "Category", "Test" },
                { "UnitPrice", "15.00" },
                { "LowStockThreshold", "10" }
            };

            var response = await Client.PostAsync($"/Products/Edit/{product.ProductId}", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            Context.ChangeTracker.Clear();

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.Name.Should().Be("Updated Product");
        }

        [Fact]
        public async Task Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            var formData = new Dictionary<string, string>
            {
                { "ProductId", "1" },
                { "Name", "Product" },
                { "SKU", "PROD-001" },
                { "Category", "Test" },
                { "UnitPrice", "10.00" },
                { "LowStockThreshold", "5" }
            };

            var response = await Client.PostAsync("Products/Edit/999", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_POST_ShouldUpdateSupplier()
        {
            ClearDatabase();
            var supplier1 = new Supplier
            {
                Name = "Supplier 1",
                ContactPerson = "John",
                Email = "john@s1.com",
                Phone = "111"
            };

            var supplier2 = new Supplier
            {
                Name = "Supplier 2",
                ContactPerson = "Jane",
                Email = "jane@s2.com",
                Phone = "222"
            };

            Context.Suppliers.AddRange(supplier1, supplier2);
            await Context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Product",
                SKU = "PROD-001",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5,
                SupplierId = supplier1.SupplierId
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "ProductId", product.ProductId.ToString() },
                { "Name", "Product" },
                { "SKU", "PROD-001" },
                { "Category", "Test" },
                { "UnitPrice", "10.00" },
                { "LowStockThreshold", "5" },
                { "SupplierId", supplier2.SupplierId.ToString() }
            };

            var response = await Client.PostAsync($"/Products/Edit/{product.ProductId}", new FormUrlEncodedContent(formData));

            Context.ChangeTracker.Clear();

            var updatedProduct = await Context.Products.FindAsync(product.ProductId);
            updatedProduct!.SupplierId.Should().Be(supplier2.SupplierId);
        }
    }
}
