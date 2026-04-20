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
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepository = new Mock<IProductRepository>();
            _service = new ProductService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Name = "Product A",
                    SKU = "SKU-001",
                    Category = "Category 1",
                    UnitPrice = 10.00m,
                    CurrentStock = 50,
                    LowStockThreshold = 10,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Product B",
                    SKU = "SKU-002",
                    Category = "Category 2",
                    UnitPrice = 20.00m,
                    CurrentStock = 30,
                    LowStockThreshold = 5,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
            };

            _mockRepository.Setup(r => r.GetAllWithSuppliersAsync())
                .ReturnsAsync(products);

            var result = await _service.GetAllProductsAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(p  => p.Name == "Product A");
            result.Should().Contain(p => p.Name == "Product B");
            _mockRepository.Verify(r => r.GetAllWithSuppliersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 15.00m,
                CurrentStock = 100,
                LowStockThreshold = 20,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _mockRepository.Setup(r => r.GetByIdWithSupplierAsync(1))
                .ReturnsAsync(product);

            var result = await _service.GetProductByIdAsync(1);

            result.Should().NotBeNull();
            result!.ProductId.Should().Be(1);
            result.Name.Should().Be("Test Product");
            result.SKU.Should().Be("TEST-001");
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
        {
            _mockRepository.Setup(r => r.GetByIdWithSupplierAsync(999))
                .ReturnsAsync((Product?)null);

            var result = await _service.GetProductByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateProductAsync_WithValidDataAndUniqueSku_ShouldCreateProduct()
        {
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                SKU = "NEW-001",
                Description = "A new product",
                Category = "New Category",
                UnitPrice = 99.99m,
                LowStockThreshold = 10,
                SupplierId = 1
            };

            _mockRepository.Setup(r => r.IsSkuUniqueAsync("NEW-001", null))
                .ReturnsAsync(true);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask)
                .Callback<Product>(p =>
                {
                    p.ProductId = 1;
                    p.CreatedDate = DateTime.Now;
                    p.ModifiedDate = DateTime.Now;
                });

            _mockRepository.Setup(r => r.GetByIdWithSupplierAsync(It.IsAny<int>()))
                .ReturnsAsync(new Product
                {
                    ProductId = 1,
                    Name = "New Product",
                    SKU = "NEW-001",
                    Category = "New Category",
                    UnitPrice = 99.99m,
                    LowStockThreshold = 10,
                    SupplierId = 1
                });

            var result = await _service.CreateProductAsync(createDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("New Product");
            result.SKU.Should().Be("NEW-001");
            result.CurrentStock.Should().Be(0);
            _mockRepository.Verify(r => r.AddAsync(It.Is<Product>(p =>
                p.Name == "New Product" &&
                p.SKU == "NEW-001" &&
                p.CurrentStock == 0
            )), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithDuplicateSku_ShouldThrowException()
        {
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                SKU = "DUPLICATE-SKU",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            _mockRepository.Setup(r => r.IsSkuUniqueAsync("DUPLICATE-SKU", null))
                .ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateProductAsync(createDto));

            exception.Message.Should().Contain("DUPLICATE-SKU");
            exception.Message.Should().Contain("already exists");

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
            _mockRepository.Verify(r => r.IsSkuUniqueAsync("DUPLICATE-SKU", null), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductExists_ShouldUpdateProduct()
        {
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Old Name",
                SKU = "OLD-001",
                Category = "Old Category",
                UnitPrice = 10.00m,
                CurrentStock = 50,
                LowStockThreshold = 10,
                CreatedDate = DateTime.Now.AddDays(-10),
                ModifiedDate = DateTime.Now.AddDays(-10)
            };

            var updateDto = new UpdateProductDto
            {
                ProductId = 1,
                Name = "Updated Name",
                SKU = "UPD-001",
                Category = "Updated Category",
                UnitPrice = 15.00m,
                LowStockThreshold = 20,
                SupplierId = 2,
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingProduct);

            _mockRepository.Setup(r => r.IsSkuUniqueAsync("UPD-001", 1))
                .ReturnsAsync(true);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _mockRepository.Setup(r => r.GetByIdWithSupplierAsync(1))
                .ReturnsAsync(existingProduct);

            var result = await _service.UpdateProductAsync(updateDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Name.Should().Be("UPD-001");
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<Product>(p =>
                p.ProductId == 1 &&
                p.Name == "Updated Name" &&
                p.SKU == "UPD-001"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldThrowException()
        {
            var updateDto = new UpdateProductDto
            {
                ProductId = 999,
                Name = "Updated Name",
                SKU = "UPD-001",
                Category = "Updated Category",
                UnitPrice = 15.00m,
                LowStockThreshold = 10,
            };

            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateProductAsync(updateDto));
        }

        [Fact]
        public async Task UpdateProductAsync_WithDuplicateSku_ShouldThrowException()
        {
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Product",
                SKU = "ORIG-001",
                Category = "Category",
                UnitPrice = 10.00m,
                LowStockThreshold= 5
            };

            var updateDto = new UpdateProductDto
            {
                ProductId = 1,
                Name = "Updated Name",
                SKU = "DUPLICATE-SKU",
                Category = "Category",
                UnitPrice = 15.00m,
                LowStockThreshold = 10,
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingProduct);

            _mockRepository.Setup(r => r.IsSkuUniqueAsync("DUPLICATE-SKU", null))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateProductAsync(updateDto));

            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductExistsAndHasNoMovements_ShouldDeleteProduct()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "Test-001",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _mockRepository.Setup(r => r.HasStockMovementsAsync(1))
                .ReturnsAsync(false);

            _mockRepository.Setup(r => r.DeleteAsync(product))
                .Returns(Task.CompletedTask);

            var result = await _service.DeleteProductAsync(1);

            result.Should().BeTrue();
            _mockRepository.Verify(r => r.DeleteAsync(product), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductHasStockMovements_ShouldThrowException()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            _mockRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            _mockRepository.Setup(r => r.HasStockMovementsAsync(1))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(()  => _service.DeleteProductAsync(1));

            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductDoesNotExist_ShouldReturnFalse()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            var result = await _service.DeleteProductAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnOnlyLowStockProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Name = "Low Stock Product",
                    SKU = "LOW-001",
                    UnitPrice = 10m,
                    LowStockThreshold = 10,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,

                }
            };

            _mockRepository.Setup(r => r.GetLowStockProductsAsync())
                .ReturnsAsync(products);

            var result = await _service.GetLowStockProductsAsync();

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Low Stock Product");
            result.First().IsLowStock.Should().BeTrue();
        }

        [Fact]
        public async Task SearchProductsAsync_ShouldReturnMatchingProducts()
        {
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Name = "Wireless Mouse",
                    SKU = "WM-001",
                    Category = "Accessories",
                    UnitPrice = 29.99m,
                    CurrentStock = 50,
                    LowStockThreshold = 10,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            };

            _mockRepository.Setup(r => r.SearchProductsAsync("Mouse", null))
                .ReturnsAsync(products);

            var result = await _service.SearchProductsAsync("Mouse");

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Wireless Mouse");
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnDistinctCategories()
        {
            var categories = new List<string> { "Electronics", "Office", "Accessories" };

            _mockRepository.Setup(r => r.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            var result = await _service.GetAllCategoriesAsync();

            result.Should().HaveCount(3);
            result.Should().Contain("Electronics");
            result.Should().Contain("Office");
            result.Should().Contain("Accessories");
        }

        [Fact]
        public async Task IsSkuUniqueAsync_ShouldReturnRepositoryResult()
        {
            _mockRepository.Setup(r => r.IsSkuUniqueAsync("NEW-SKU", null))
                .ReturnsAsync(true);

            var result = await _service.IsSkuUniqueAsync("NEW-SKU");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanDeleteProductAsync_WhenNoMovements_ShouldReturnTrue()
        {
            _mockRepository.Setup(r => r.HasStockMovementsAsync(1))
                .ReturnsAsync(false);

            var result = await _service.CanDeleteProductAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanDeleteProductAsync_WhenMovements_ShouldReturnFalse()
        {
            _mockRepository.Setup(r => r.HasStockMovementsAsync(1))
                .ReturnsAsync(false);

            var result = await _service.CanDeleteProductAsync(1);

            result.Should().BeTrue();
        }
    }
}
