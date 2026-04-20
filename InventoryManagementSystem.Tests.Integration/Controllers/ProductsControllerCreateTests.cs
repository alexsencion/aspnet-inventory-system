using Azure;
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
    public class ProductsControllerCreateTests : IntegrationTestBase
    {
        public ProductsControllerCreateTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_GET_ShouldReturnCreateForm()
        {
            var response = await Client.GetAsync("/Products/Create");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Create New Product");
            content.Should().Contain("Product Name");
            content.Should().Contain("SKU");
            content.Should().Contain("Category");
            content.Should().Contain("Unit Price");
        }

        [Fact]
        public async Task Create_GET_ShouldIncludeSupplierDropdown()
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

            var response = await Client.GetAsync("/Products/Create");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Supplier");
        }

        [Fact]
        public async Task Create_POST_WithValidData_ShouldCreateProduct()
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

            var formData = new Dictionary<string, string>
            {
                { "Name", "New Test Product" },
                { "SKU", "NTP-001" },
                { "Description", "A new test product" },
                { "Category", "Test Category" },
                { "UnitPrice", "49.99" },
                { "LowStockThreshold", "15" },
                { "SupplierId", supplier.SupplierId.ToString() }
            };

            var response = await Client.PostAsync("/Products/Create", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            var createdProduct = Context.Products.FirstOrDefault(p => p.SKU == "NTP-001");
            createdProduct.Should().NotBeNull();
            createdProduct!.Name.Should().Be("New Test Product");
            createdProduct.CurrentStock.Should().Be(0);
            createdProduct.UnitPrice.Should().Be(49.99m);
        }

        [Fact]
        public async Task Create_POST_WithDuplicateSKU_ShouldShowError()
        {
            ClearDatabase();
            var existingProduct = new Product
            {
                Name = "Existing Product",
                SKU = "DUPLICATE-SKU",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };
            Context.Products.Add(existingProduct);
            await Context.SaveChangesAsync();

            var formData = new Dictionary<string, string>
            {
                { "Name", "Another Product" },
                { "SKU", "DUPLICATE-SKU" },
                { "Category", "Test" },
                { "UnitPrice", "20.00" },
                { "LowStockThreshold", "10" }
            };

            var response = await Client.PostAsync("/Products/Create", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine(content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("already exists");
        }

        [Fact]
        public async Task Create_POST_WithInvalidData_ShouldReturnValidationErrors()
        {
            var formData = new Dictionary<string, string>
            {
                { "Name", "" },
                { "SKU", "" },
                { "Category", "" },
                { "UnitPrice", "-10" },
                { "LowStockThreshold", "-5" }
            };

            var response = await Client.PostAsync("/Products/Create", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Create New Product");
        }

        [Fact]
        public async Task Create_POST_WithoutSupplier_ShouldCreateProduct()
        {
            ClearDatabase();
            var formData = new Dictionary<string, string>
            {
                { "Name", "Product Without Supplier" },
                { "SKU", "PWS-001" },
                { "Category", "Test" },
                { "UnitPrice", "30.00" },
                { "LowStockThreshold", "10" },
                { "SupplierId", "" }
            };

            var response = await Client.PostAsync("/Products/Create", new FormUrlEncodedContent(formData));

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Redirect, HttpStatusCode.Found);

            var createdProduct = Context.Products.FirstOrDefault(p => p.SKU == "PWS-001");
            createdProduct.Should().NotBeNull();
            createdProduct!.SupplierId.Should().BeNull();
        }
    }
}
