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
    public class SuppliersControllerEditTests : IntegrationTestBase
    {
        public SuppliersControllerEditTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Edit_GET_WithValidId_ShouldReturnEditForm()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Original Supplier",
                ContactPerson = "Original Person",
                Email = "original@test.com",
                Phone = "111-222-3333"
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync($"/Suppliers/Edit/{supplier.SupplierId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Edit Supplier");
            content.Should().Contain("Original Supplier");
            content.Should().Contain("original@test.com");
        }

        [Fact]
        public async Task Edit_GET_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.GetAsync("/Suppliers/Edit/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Edit_POST_WithValidData_ShouldUpdateSupplier()
        {
            // Arrange
            ClearDatabase();
            var supplier = new Supplier
            {
                Name = "Original Supplier",
                ContactPerson = "Original Person",
                Email = "original@test.com",
                Phone = "111-222-3333",
                CreatedDate = System.DateTime.Now
            };

            Context.Suppliers.Add(supplier);
            await Context.SaveChangesAsync();
            var supplierId = supplier.SupplierId; // Save the ID

            // Get the edit form and extract token
            var getResponse = await Client.GetAsync($"/Suppliers/Edit/{supplierId}");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "SupplierId", supplierId.ToString() },
                { "Name", "Updated Supplier" },
                { "ContactPerson", "Updated Person" },
                { "Email", "updated@test.com" },
                { "Phone", "999-888-7777" },
                { "Address", "New Address" },
                { "Notes", "Updated notes" }
            };

            // Act
            var response = await Client.PostAsync($"/Suppliers/Edit/{supplierId}",
                new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);

            // Refresh context to get updated data
            RefreshContext();

            // Verify supplier was updated
            var updatedSupplier = await Context.Suppliers.FindAsync(supplierId);
            updatedSupplier.Should().NotBeNull();
            updatedSupplier!.Name.Should().Be("Updated Supplier");
            updatedSupplier.Email.Should().Be("updated@test.com");
            updatedSupplier.Phone.Should().Be("999-888-7777");
            updatedSupplier.Address.Should().Be("New Address");
        }

        [Fact]
        public async Task Edit_POST_WithMismatchedId_ShouldReturnNotFound()
        {
            // Arrange
            var getResponse = await Client.GetAsync("/Suppliers/Create");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "SupplierId", "1" },
                { "Name", "Test Supplier" },
                { "ContactPerson", "Test Person" },
                { "Email", "test@test.com" },
                { "Phone", "111-222-3333" }
            };

            // Act
            var response = await Client.PostAsync("/Suppliers/Edit/999", new FormUrlEncodedContent(formData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
