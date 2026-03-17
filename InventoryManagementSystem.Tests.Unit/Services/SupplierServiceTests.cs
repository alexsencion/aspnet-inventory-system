using FluentAssertions;
using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Tests.Unit.Services
{
    public class SupplierServiceTests
    {
        private readonly Mock<ISupplierRepository> _mockRepository;
        private readonly SupplierService _service;

        public SupplierServiceTests()
        {
            _mockRepository = new Mock<ISupplierRepository>();
            _service = new SupplierService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllSuppliersAsync_ShouldReturnAllSuppliers()
        {
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    SupplierId = 1,
                    Name = "Supplier A",
                    ContactPerson = "John",
                    Email = "john@a.com",
                    Phone = "111",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Supplier
                {
                    SupplierId = 2,
                    Name = "Supplier B",
                    ContactPerson = "Jane",
                    Email = "john@b.com",
                    Phone = "222",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(suppliers);

            var result = await _service.GetAllSuppliersAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Supplier A");
            result.Should().Contain(s => s.Name == "Supplier B");
            _mockRepository.Verify(r =>  r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetSupplierByIdAsync_WhenSupplierExists_ShouldReturnSupplier()
        {
            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-222-3333",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Products = new List<Product>()
            };

            _mockRepository.Setup(r => r.GetSupplierWithProductsAsync(1))
                .ReturnsAsync(supplier);

            var result = await _service.GetSupplierByIdAsync(1);

            result.Should().NotBeNull();
            result!.SupplierId.Should().Be(1);
            result.Name.Should().Be("Test Supplier");
            _mockRepository.Verify(r => r.GetSupplierWithProductsAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetSupplierByIdAsync_WhenSupplierDoesNotExist_ShouldReturnNull()
        {
            _mockRepository.Setup(r => r.GetSupplierWithProductsAsync(999))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.GetSupplierByIdAsync(999);

            result.Should().BeNull();
        }

        public async Task CreateSupplierAsync_WithValidData_ShouldCreateSupplier()
        {
            var createdDto = new CreateSupplierDto
            {
                Name = "New Supplier",
                ContactPerson = "New Contact",
                Email = "new@supplier.com",
                Phone = "555-555-5555",
                Address = "123 Test St",
                Notes = "Test notes"
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Supplier>()))
                .Returns(Task.CompletedTask)
                .Callback<Supplier>(s =>
                {
                    s.SupplierId = 1;
                    s.CreatedDate = DateTime.Now;
                    s.ModifiedDate = DateTime.Now;
                });

            var result = await _service.CreateSupplierAsync(createdDto);

            result.Should().BeNull();
            result.Name.Should().Be("New Supplier");
            result.Email.Should().Be("new@supplier.com");
            _mockRepository.Verify(r => r.AddAsync(It.Is<Supplier>(s =>
                s.Name == "New Supplier" &&
                s.ContactPerson == "New Contact" &&
                s.Email == "new@supplier.com"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenSupplierExists_ShouldUpdateSupplier()
        {
            var existingSupplier = new Supplier
            {
                SupplierId = 1,
                Name = "Old Name",
                ContactPerson = "Old Contact",
                Email = "old@supplier.com",
                Phone = "111-111-1111",
                CreatedDate = DateTime.Now.AddDays(-10),
                ModifiedDate = DateTime.Now.AddDays(-10)
            };

            var updateDto = new UpdateSupplierDto
            {
                SupplierId = 1,
                Name = "Updated Name",
                ContactPerson = "Updated Contact",
                Email = "updated@supplier.com",
                Phone = "222-222-2222"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingSupplier);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Supplier>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateSupplierAsync(updateDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Email.Should().Be("updated@supplier.com");
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Supplier>(s =>
                s.SupplierId == 1 &&
                s.Name == "Updated Name" &&
                s.Email == "updated@supplier.com"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateSupplierAsync_WhenSupplierDoesNotExist_ShouldThrowException()
        {
            var updateDto = new UpdateSupplierDto
            {
                SupplierId = 1,
                Name = "Updated Contact",
                Email = "updated@supplier.com",
                Phone = "222-222-2222"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Supplier?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateSupplierAsync(updateDto));
        }

        [Fact]
        public async Task DeleteSupplierAsync_WhenSupplierExistsAndHasNoProducts_ShouldDeleteSupplier()
        {
            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111",
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(supplier);

            _mockRepository.Setup(r => r.HasProductAsync(1))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.DeleteAsync(supplier))
                .Returns(Task.CompletedTask);

            var result = await _service.DeleteSupplierAsync(1);

            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(supplier), Times.Once);
        }

        [Fact]
        public async Task DeleteSupplierAsync_WhenSupplierHasProducts_ShouldThrowException()
        {
            var supplier = new Supplier
            {
                SupplierId = 1,
                Name = "Test Supplier",
                ContactPerson = "Test Person",
                Email = "test@supplier.com",
                Phone = "111-111-1111",
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(supplier);

            _mockRepository.Setup(r => r.HasProductAsync(1))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.DeleteSupplierAsync(1));

            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Supplier>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSupplierAsync_WhenSupplierDoesNotExist_ShouldReturnFalse()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Supplier?)null);

            var result = await _service.DeleteSupplierAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SearchSuppliersAsync_ShouldReturnMatchingSuppliers()
        {
            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    SupplierId = 1,
                    Name = "Tech Supplier",
                    ContactPerson = "John",
                    Email = "john@tech.com",
                    Phone = "111",
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                }
            };

            _mockRepository.Setup(r => r.SearchByNameAsync("Tech"))
                .ReturnsAsync(suppliers);

            var result = await _service.SearchSupplierAsync("Tech");

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Tech Supplier");
        }

        [Fact]
        public async Task CanDeleteSupplierAsync_WhenSupplierHasNoProducts_ShouldReturnTrue()
        {
            _mockRepository.Setup(r => r.HasProductAsync(1))
                .ReturnsAsync(false);

            var result = await _service.CanDeleteSupplierAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanDeleteSupplierAsync_WhenSupplierHasProducts_ShouldReturnFalse()
        {
            _mockRepository.Setup(r => r.HasProductAsync(1))
                .ReturnsAsync(true);

            var result = await _service.CanDeleteSupplierAsync(1);

            result.Should().BeFalse();
        }
    }
}
