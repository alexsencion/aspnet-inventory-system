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
    public class ProductRepositoryTests : TestBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests() 
        {
            _context = GetInMemoryDbContext();
            _repository = new ProductRepository(_context);
        }

        [Fact]
        public async Task GetAllWithSupplierAsync_ShouldReturnAllProductsWithSuppliers()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "john@supplier.com",
                Phone = "111-222-3333"
            };
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var products = new[]
            {
                new Product
                {
                    Name = "Product A",
                    SKU = "SKU-001",
                    Category = "Category 1",
                    UnitPrice = 10.00m,
                    LowStockThreshold = 5,
                    SupplierId = supplier.SupplierId
                },

                new Product
                {
                    Name = "Product B",
                    SKU = "SKU-002",
                    Category = "Category 2",
                    UnitPrice = 20.00m,
                    LowStockThreshold = 10
                },
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllWithSuppliersAsync();

            result.Should().HaveCount(2);
            result.First().Supplier.Should().NotBeNull();
            result.First().Supplier!.Name.Should().Be("Test Supplier");
        }

        [Fact]
        public async Task GetByIdWithSupplierAsync_WhenProductExists_ShouldReturnProductWithSupplier()
        {
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                ContactPerson = "John Doe",
                Email = "john@supplier.com",
                Phone = "111-222-3333"
            };
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test Category",
                UnitPrice = 15.00m,
                CurrentStock = 50,
                LowStockThreshold = 10,
                SupplierId = supplier.SupplierId
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdWithSupplierAsync(product.ProductId);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Product");
            result.Supplier.Should().NotBeNull();
            result.Supplier!.Name.Should().Be("Test Supplier");
        }

        [Fact]
        public async Task SearchProductsAsync_WithSearchTerm_ShouldReturnMatchingProducts()
        {
            var products = new[]
            {
                new Product
                {
                    Name = "Wireless Mouse",
                    SKU = "WM-001",
                    Description = "Ergonomic wireless mouse",
                    Category = "Accessories",
                    UnitPrice = 29.99m,
                    LowStockThreshold = 10
                },
                new Product
                {
                    Name = "Mechanical Keyboard",
                    SKU = "KB-001",
                    Description = "RGB mechanical keyboard",
                    Category = "Accessories",
                    UnitPrice = 89.99m,
                    LowStockThreshold = 5
                },
                new Product
                {
                    Name = "USB-C Cable",
                    SKU = "CBL-001",
                    Description = "6ft USB-C cable",
                    Category = "Accesories",
                    UnitPrice = 12.99m,
                    LowStockThreshold = 20
                },
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchProductsAsync("Mouse");

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Wireless Mouse");
        }

        [Fact]
        public async Task SearchProductsAsync_WithCategory_ShouldFilterByCategory()
        {
            var products = new[]
            {
                new Product { Name = "Product 1", SKU = "P1", Category = "Electronics", UnitPrice = 10m, LowStockThreshold = 5 },
                new Product { Name = "Product 2", SKU = "P2", Category = "Electronics", UnitPrice = 20m, LowStockThreshold = 5 },
                new Product { Name = "Product 3", SKU = "P3", Category = "Office", UnitPrice = 30m, LowStockThreshold = 5 },
            };

            await _context.Products.AddRangeAsync( products);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchProductsAsync("", "Electronics");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(p => p.Category == "Electronics");
        }

        [Fact]
        public async Task SearchProductsAsync_WithSearchTermAndCategory_ShouldApplyBothFilters()
        {
            var products = new[]
            {
                new Product { Name = "Wireless Mouse", SKU = "WM-001", Category = "Accessories", UnitPrice = 10m, LowStockThreshold = 5 },
                new Product { Name = "Wired Mouse", SKU = "WM-002", Category = "Accessories", UnitPrice = 15m, LowStockThreshold = 5 },
                new Product { Name = "Wireless Keyboard", SKU = "KB-001", Category = "Accessories", UnitPrice = 50m, LowStockThreshold = 5 },
                new Product { Name = "Wireless Headset", SKU = "HS-001", Category = "Audio", UnitPrice = 80m, LowStockThreshold = 5 },
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchProductsAsync("Wireless", "Accessories");

            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "Wireless Mouse");
            result.Should().Contain(p => p.Name == "Wireless Keyboard");
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnOnlyLowStockItems()
        {
            var products = new[]
            {
                new Product
                {
                    Name = "Low Stock Product",
                    SKU = "LOW-001",
                    Category = "Test",
                    UnitPrice = 10m,
                    CurrentStock = 3,
                    LowStockThreshold = 10
                },
                new Product
                {
                    Name = "Normal Stock Product",
                    SKU = "NRM-001",
                    Category = "Test",
                    UnitPrice = 20m,
                    CurrentStock = 50,
                    LowStockThreshold = 10
                },
                new Product
                {
                    Name = "Out of Stock Product",
                    SKU = "OUT-001",
                    Category = "Test",
                    UnitPrice = 15m,
                    CurrentStock = 0,
                    LowStockThreshold = 5
                }
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = await _repository.GetLowStockProductsAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "Low Stock Product");
            result.Should().Contain(p => p.Name == "Out of Stock Product");
            result.Should().NotContain(p => p.Name == "Normal Stock Product");
        }

        [Fact]
        public async Task IsSkuUniqueAsync_WhenSkuDoesNotExist_ShouldReturnTrue()
        {
            var product = new Product
            {
                Name = "Existing Product",
                SKU = "EXIST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.IsSkuUniqueAsync("NEW-SKU");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSkuUniqueAsync_WhenSkuExists_ShouldReturnFalse()
        {
            var product = new Product
            {
                Name = "Existing Product",
                SKU = "EXIST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.IsSkuUniqueAsync("EXIST-001");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSkuUniqueAsync_WhenSkuExistsButExcluded_ShouldReturnTrue()
        {
            var product = new Product
            {
                Name = "Existing Product",
                SKU = "EXIST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.IsSkuUniqueAsync("EXIST-001", product.ProductId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnDistinctionCategories()
        {
            var products = new[]
            {
                new Product { Name = "P1", SKU = "S1", Category = "Electronics", UnitPrice = 10m, LowStockThreshold = 5 },
                new Product { Name = "P2", SKU = "S2", Category = "Electronics", UnitPrice = 20m, LowStockThreshold = 5 },
                new Product { Name = "P3", SKU = "S3", Category = "Office", UnitPrice = 30m, LowStockThreshold = 5 },
                new Product { Name = "P4", SKU = "S3", Category = "Accessories", UnitPrice = 40m, LowStockThreshold = 5 },
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllCategoriesAsync();

            result.Should().HaveCount(3);
            result.Should().Contain("Electronics");
            result.Should().Contain("Office");
            result.Should().Contain("Accessories");
        }

        [Fact]
        public async Task HasStockMovementsAsync_WhenProductHasMovements_ShouldReturnTrue()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "EXIST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 10,
                LowStockThreshold = 5
            };

            await _context.Products.AddRangeAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 10,
                MovementDate = DateTime.Now,
                StockAfterMovement = 10,
            };

            await _context.StockMovements.AddAsync(movement);
            await _context.SaveChangesAsync();

            var result = await _repository.HasStockMovementsAsync(product.ProductId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasStockMovementsAsync_WhenProductHasNoMovements_ShouldReturnFalse()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "EXIST-001",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5
            };

            await _context.Products.AddRangeAsync(product);
            await _context.SaveChangesAsync();

            var result = await _repository.HasStockMovementsAsync(product.ProductId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_ShouldAddProductToDatabase()
        {
            var product = new Product
            {
                Name = "New Product",
                SKU = "NEW-001",
                Description = "A new product",
                Category = "New Category",
                UnitPrice = 99.99m,
                CurrentStock = 0,
                LowStockThreshold = 10
            };

            await _repository.AddAsync(product);

            var savedProduct = await _context.Products.FindAsync(product.ProductId);
            savedProduct.Should().NotBeNull();
            savedProduct!.Name.Should().Be("New Product");
            savedProduct.SKU.Should().Be("NEW-001");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingProduct()
        {
            var product = new Product
            {
                Name = "Original Name",
                SKU = "ORIG-001",
                Category = "Original Category",
                UnitPrice = 10.00m,
                LowStockThreshold = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            product.Name = "Updated Name";
            product.UnitPrice = 15.00m;
            await _repository.UpdateAsync(product);

            var updatedProduct = await _context.Products.FindAsync(product.ProductId);
            updatedProduct.Should().NotBeNull();
            updatedProduct!.Name.Should().Be("Updated Name");
            updatedProduct.UnitPrice.Should().Be(15.00m);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveProductFromDatabase()
        {
            var product = new Product
            {
                Name = "To Be Deleted",
                SKU = "DELETE-001",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            var productId = product.ProductId;

            await _repository.DeleteAsync(product);

            var deletedProduct = await _context.Products.FindAsync(productId);
            deletedProduct.Should().BeNull();
        }
    }
}
