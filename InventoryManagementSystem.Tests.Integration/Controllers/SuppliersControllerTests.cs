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
    public class SuppliersControllerTests : IntegrationTestBase
    {
        public SuppliersControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        { 
        }

        [Fact]
        public async Task Index_ShouldReturnSuccessStatusCode()
        {
            var response = await Client.GetAsync("/Suppliers");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Index_WithSuppliers_ShouldDisplaySuppliers()
        {
            ClearDatabase();
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "Test Supplier 1",
                    ContactPerson = "John Doe",
                    Email = "john@test.com",
                    Phone = "111-222-3333"
                },
                new Supplier
                {
                    Name = "Test Supplier 2",
                    ContactPerson = "Jane Smith",
                    Email = "jane@test.com",
                    Phone = "444-555-6666"
                },
            };

            Context.Suppliers.AddRange(suppliers);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Suppliers");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Test Supplier 1");
            content.Should().Contain("Test Supplier 2");
            content.Should().Contain("john@test.com");
            content.Should().Contain("jane@test.com");
        }

        [Fact]
        public async Task Index_WithSearchTerm_ShouldFilterResults()
        {
            ClearDatabase();
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "Tech Distributors",
                    ContactPerson = "John Tech",
                    Email = "john@tech.com",
                    Phone = "111-222-3333"
                },
                new Supplier
                {
                    Name = "Office Supplies Co",
                    ContactPerson = "Jane Office",
                    Email = "jane@office.com",
                    Phone = "444-555-6666"
                }
            };

            Context.Suppliers.AddRange(suppliers);
            await Context.SaveChangesAsync();

            var response = await Client.GetAsync("/Suppliers?searchTerm=Tech");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Tech Distributors");
            content.Should().NotContain("Office Supplies Co");
        }

        [Fact]
        public async Task Index_WithNoSuppliers_ShouldShowEmptyMessage()
        {
            ClearDatabase();

            var response = await Client.GetAsync("/Suppliers");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("No suppliers found");
        }
    }
}
