using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class SuppliersControllerDetailsTests : IntegrationTestBase
    {
        public SuppliersControllerDetailsTests(CustomWebApplicationFactory<Program> factory) : base(factory) 
        {
        }

        [Fact]
        public async Task Details_WithValidId_ShouldReturnSupplierDetails()
        {
            ClearDatabase();

            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "john@test.com",
                Phone = "111-222-3333",
                Address = "123 Test Street",
                Notes = "Test notes"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Suppliers/Details/{supplier.SupplierId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Supplier");
            content.Should().Contain("John Doe");
            content.Should().Contain("john@test.com");
            content.Should().Contain("111-222-3333");
            content.Should().Contain("123 Test Street");
            content.Should().Contain("Test notes");
        }

        [Fact]
        public async Task Details_WithInValidId_ShouldReturnNotFound()
        {
            var response = await Client.GetAsync("/Suppliers/Details/999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Details_WithProducts_ShouldShowProductCount()
        {
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "john@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var products = new[]
            {
                new Product
                {
                    Name = "Product 1",
                    SKU = "SKU-001",
                    Category = "Category A",
                    UnitPrice = 10.00m,
                    LowStockThreshold = 5,
                    SupplierId = supplier.SupplierId,
                },
                new Product
                {
                    Name = "Product 2",
                    SKU = "SKU-002",
                    Category = "Category B",
                    UnitPrice = 20.00m,
                    LowStockThreshold = 10,
                    SupplierId = supplier.SupplierId,
                }
            };

            Context.Products.AddRange(products);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync($"/Suppliers/Details/{supplier.SupplierId}");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);


            content.Should().ContainAll(
                "<dd class=\"col-sm-8\">",
                "2" // The actual count
            );
        }
    }
}
