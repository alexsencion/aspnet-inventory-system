using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Tests.Integration.Helpers;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Integration.Controllers
{
    public class SuppliersControllerDeleteTests : IntegrationTestBase
    {
        public SuppliersControllerDeleteTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Delete_GET_WithValidId_ShouldReturnDeleteConfirmation()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Supplier To Delete",
                ContactPerson = "Delete Person",
                Email = "delete@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/Suppliers/Delete/{supplier.SupplierId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Delete Supplier");
            content.Should().Contain("Supplier To Delete");
            content.Should().Contain("delete@test.com");
        }

        [Fact]
        public async Task Delete_GET_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.GetAsync("/Suppliers/Delete/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_GET_WithSupplierHavingProducts_ShouldShowWarning()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Supplier With Products",
                ContactPerson = "Test Person",
                Email = "test@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 10.00m,
                LowStockThreshold = 5,
                SupplierId = supplier.SupplierId
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/Suppliers/Delete/{supplier.SupplierId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("cannot be deleted");
            content.Should().Contain("associated products");
        }

        [Fact]
        public async Task Delete_POST_WithValidIdAndNoProducts_ShouldDeleteSupplier()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Supplier To Delete",
                ContactPerson = "Delete Person",
                Email = "delete@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();
            var supplierId = supplier.SupplierId;

            // Get delete page and extract token
            var getResponse = await Client.GetAsync($"/Suppliers/Delete/{supplierId}");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "SupplierId", supplierId.ToString() }
            };

            // Act
            var response = await Client.PostAsync($"/Suppliers/Delete/{supplierId}",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);

            // Refresh context to get latest data
            RefreshContext();

            // Verify supplier was deleted
            var deletedSupplier = await Context.Suppliers.FindAsync(supplierId);
            deletedSupplier.Should().BeNull();
        }

        [Fact]
        public async Task Delete_POST_WithSupplierHavingProducts_ShouldNotDelete()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Supplier With Products",
                ContactPerson = "Test Person",
                Email = "test@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 10.00m,
                LowStockThreshold = 5,
                SupplierId = supplier.SupplierId
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();

            // Get delete page and extract token
            var getResponse = await Client.GetAsync($"/Suppliers/Delete/{supplier.SupplierId}");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "SupplierId", supplier.SupplierId.ToString() }
            };

            // Act
            var response = await Client.PostAsync($"/Suppliers/Delete/{supplier.SupplierId}",
                new FormUrlEncodedContent(formData));

            // Assert
            // Supplier should still exist
            var existingSupplier = await Context.Suppliers.FindAsync(supplier.SupplierId);
            existingSupplier.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_POST_WithInvalidId_ShouldReturnRedirect()
        {
            // Arrange
            var getResponse = await Client.GetAsync("/Suppliers/Create");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "SupplierId", "999" }
            };

            // Act
            var response = await Client.PostAsync("/Suppliers/Delete/999", new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found, HttpStatusCode.OK);
        }
    }
}
