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
    public class ProductsControllerDetailsTests : IntegrationTestBase
    {
        public ProductsControllerDetailsTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Details_WithValidId_ShouldReturnProductDetails()
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

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Description = "A test product description",
                Category = "Test Category",
                UnitPrice = 99.99m,
                CurrentStock = 75,
                LowStockThreshold = 20,
                SupplierId = supplier.SupplierId
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Details/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Product");
            content.Should().Contain("TEST-001");
            content.Should().Contain("A test product description");
            content.Should().Contain("Test Category");
            content.Should().Contain("$99.99");
            content.Should().Contain("75");
            content.Should().Contain("Test Supplier");
        }

        [Fact]
        public async Task Details_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await Client.GetAsync("/Products/Details/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Details_WithLowStockProduct_ShouldShowWarning()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Low Stock Product",
                SKU = "LOW-001",
                Category = "Test",
                UnitPrice = 50.00m,
                CurrentStock = 5,
                LowStockThreshold = 20,
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Details/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Low Stock Alert");
        }

        [Fact]
        public async Task Details_WithProductWithoutSupplier_ShouldShowNoSupplier()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Product Without Supplier",
                SKU = "PWS-001",
                Category = "Test",
                UnitPrice = 25.00m,
                CurrentStock = 30,
                LowStockThreshold = 10,
                SupplierId = null
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Details/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("No supplier assigned");
        }

        [Fact]
        public async Task Details_ShouldShowInventoryValue()
        {
            ClearDatabase();
            var product = new Product
            {
                Name = "Value Test Product",
                SKU = "VTP-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Products/Details/{product.ProductId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("500.00 units");
        }
    }
}
