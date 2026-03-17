using FluentAssertions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Unit.Repositories
{
    public class SupplierRepositoryTests : TestBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SupplierRepository _repository;

        public SupplierRepositoryTests()
        {
            _context = GetInMemoryDbContext();
            _repository = new SupplierRepository(_context);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllSuppliers()
        {
            // Arrange
            var suppliers = new[]
            {
                new Supplier
                {
                    Name = "Supplier A",
                    ContactPerson = "John Doe",
                    Email = "john@suppliera.com",
                    Phone = "123-456-7890",
                },

                new Supplier
                {
                    Name = "Supplier B",
                    ContactPerson = "Jane Smith",
                    Email = "jane@supplierb.com",
                    Phone = "898-765-4321",
                }
            };

            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Supplier A");
            result.Should().Contain(s => s.Name == "Supplier B");
        }

        [Fact]
        public async Task GetByIdAsync_WhenSupplierExists_ShouldReturnSupplier()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-222-3333",
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(supplier.SupplierId);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Supplier");
            result!.Email.Should().Be("test@supplier.com");
        }

        [Fact]
        public async Task GetByIdAsync_WhenSupplierDoesNotExist_ShouldReturnNull()
        {
            var result = await _repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddSupplierToDatabase()
        {
            var supplier = new Supplier
            {
                Name = "New Supplier",
                ContactPerson = "New Contact",
                Email = "new@supplier.com",
                Phone = "555-555-5555",
            };

            await _repository.AddAsync(supplier);

            var savedSupplier = await _context.Suppliers.FindAsync(supplier.SupplierId);
            savedSupplier.Should().NotBeNull();
            savedSupplier!.Name.Should().Be("New Supplier");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingSupplier()
        {
            var supplier = new Supplier
            {
                Name = "Original Name",
                ContactPerson = "Original Contact",
                Email = "original@supplier.com",
                Phone = "111-111-1111",
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            supplier.Name = "Updated Name";
            supplier.Email = "updated@supplier.com";
            await _repository.UpdateAsync(supplier);

            var updatedSupplier = await _context.Suppliers.FindAsync(supplier.SupplierId);
            updatedSupplier.Should().NotBeNull();
            updatedSupplier!.Name.Should().Be("Updated Name");
            updatedSupplier.Email.Should().Be("updated@supplier.com");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveSupplierFromDatabase()
        {
            var supplier = new Supplier
            {
                Name = "To Be Deleted",
                ContactPerson = "Delete Me",
                Email = "delete@supplier.com",
                Phone = "999-999-9999"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();
            var supplierId = supplier.SupplierId;

            await _repository.DeleteAsync(supplier);

            var deletedSupplier = await _context.Suppliers.FindAsync(supplierId);
            deletedSupplier.Should().BeNull();
        }

        [Fact]
        public async Task SearchByNameAsync_WhenMatchFound_ShouldReturnMatchingSuppliers()
        {
            var suppliers = new[]
            {
                new Supplier
                {
                    Name = "Tech Distributors",
                    ContactPerson = "John Tech",
                    Email = "john@tech.com",
                    Phone = "111-111-1111"
                },
                new Supplier
                {
                    Name = "Office Suppliers",
                    ContactPerson = "Jane Office",
                    Email = "jane@office.com",
                    Phone = "222-222-2222"
                },
                new Supplier
                {
                    Name = "Tech Solutions",
                    ContactPerson = "Bob Solutions",
                    Email = "bob@solutions.com",
                    Phone = "333-333-3333"
                }
            };

            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchByNameAsync("Tech");

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Tech Distributors");
            result.Should().Contain(s => s.Name == "Tech Solutions");
        }

        [Fact]
        public async Task SearchByNameAsync_WhenNoMatch_ShouldReturnEmptyList()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchByNameAsync("NonExistent");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchByNameAsync_WithEmptySearchTerm_ShouldReturnAllSuppliers()
        {
            var suppliers = new[]
            {
                new Supplier { Name = "Supplier A", ContactPerson = "A", Email = "a@test.com", Phone = "111" },
                new Supplier { Name = "Supplier B", ContactPerson = "B", Email = "b@test.com", Phone = "222" }
            };

            await _context.Suppliers.AddRangeAsync(suppliers);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchByNameAsync("");

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetSupplierWithProductsAsync_ShouldIncludeProducts()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 10.00m,
                LowStockThreshold = 5,
                SupplierId = supplier.SupplierId,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.GetSupplierWithProductsAsync(supplier.SupplierId);

            result.Should().NotBeNull();
            result!.Products.Should().HaveCount(1);
            result.Products.First().Name.Should().Be("Test Product");
        }

        [Fact]
        public async Task HasProductsAsync_WhenSupplierHasProducts_ShouldReturnTrue()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 10.00m,
                LowStockThreshold = 5,
                SupplierId = supplier.SupplierId,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.HasProductAsync(supplier.SupplierId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasProductsAsync_WhenSupplierHasNoProducts_ShouldReturnFalse()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _repository.HasProductAsync(supplier.SupplierId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WhenSupplierExists_ShouldReturnTrue()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111"
            };

            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var result = await _repository.ExistsAsync(s => s.SupplierId == supplier.SupplierId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WhenSupplierDoesNotExist_ShouldReturnFalse()
        {
            var result = await _repository.ExistsAsync(s => s.SupplierId == 999);

            result.Should().BeFalse();
        }



    }
}
