using FluentAssertions;
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
    public class SuppliersControllerCreateTests : IntegrationTestBase
    {
        public SuppliersControllerCreateTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_GET_ShouldReturnCreateForm()
        {
            // Act
            var response = await Client.GetAsync("/Suppliers/Create");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Create New Supplier");
            content.Should().Contain("Supplier Name");
            content.Should().Contain("Contact Person");
            content.Should().Contain("Email");
            content.Should().Contain("Phone");
        }

        [Fact]
        public async Task Create_POST_WithValidData_ShouldCreateSupplier()
        {
            // Arrange
            ClearDatabase();

            // Get the create form and extract anti-forgery token
            var getResponse = await Client.GetAsync("/Suppliers/Create");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "Name", "New Test Supplier" },
                { "ContactPerson", "Test Person" },
                { "Email", "test@newsupplier.com" },
                { "Phone", "999-888-7777" },
                { "Address", "456 New Street" },
                { "Notes", "New supplier notes" }
            };

            // Act
            var postResponse = await Client.PostAsync("/Suppliers/Create", new FormUrlEncodedContent(formData));

            // Assert
            postResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);

            // Verify supplier was created in database
            var createdSupplier = Context.Suppliers.FirstOrDefault(s => s.Email == "test@newsupplier.com");
            createdSupplier.Should().NotBeNull();
            createdSupplier!.Name.Should().Be("New Test Supplier");
            createdSupplier.ContactPerson.Should().Be("Test Person");
            createdSupplier.Phone.Should().Be("999-888-7777");
        }

        [Fact]
        public async Task Create_POST_WithInvalidData_ShouldReturnValidationErrors()
        {
            // Arrange
            var getResponse = await Client.GetAsync("/Suppliers/Create");
            var token = await AntiForgeryTokenExtractor.ExtractAntiForgeryToken(getResponse);

            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", token },
                { "Name", "" }, // Missing required field
                { "ContactPerson", "Test Person" },
                { "Email", "invalid-email" }, // Invalid email format
                { "Phone", "" } // Missing required field
            };

            // Act
            var response = await Client.PostAsync("/Suppliers/Create", new FormUrlEncodedContent(formData));
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Create New Supplier");
        }
    }
}
