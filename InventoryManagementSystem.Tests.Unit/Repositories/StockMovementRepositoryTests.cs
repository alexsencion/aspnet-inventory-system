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
    public class StockMovementRepositoryTests : TestBase
    {
        private readonly ApplicationDbContext _context;
        private readonly StockMovementRepository _repository;

        public StockMovementRepositoryTests()
        {
            _context = GetInMemoryDbContext();
            _repository = new StockMovementRepository(_context);
        }

        [Fact]
        public async Task GetAllWithProductAsync_ShouldReturnMovementsWithProducts()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10.00m,
                CurrentStock = 100,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movements = new[]
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now.AddDays(-2),
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now.AddDays(-1),
                    StockAfterMovement = 40
                },
            };

            await _context.StockMovements.AddRangeAsync(movements);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllWithProductAsync();

            result.Should().HaveCount(2);
            result.All(m => m.Product != null).Should().BeTrue();
            result.First().Product.Name.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetByProductIdAsync_ShouldReturnOnlyMovementsForSpecificProduct()
        {
            var product1 = new Product
            {
                Name = "Product 1",
                SKU = "P1",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5,
            };

            var product2 = new Product
            {
                Name = "Product 2",
                SKU = "P2",
                Category = "Test",
                UnitPrice = 20m,
                LowStockThreshold = 5,
            };

            await _context.Products.AddRangeAsync(product1, product2);
            await _context.SaveChangesAsync();

            var movements = new[]
            {
                new StockMovement
                {
                    ProductId = product1.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product2.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 30,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 30
                },
                new StockMovement
                {
                    ProductId = product1.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 40
                }
            };

            await _context.StockMovements.AddRangeAsync(movements);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByProductIdAsync(product1.ProductId);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.ProductId == product1.ProductId);
        }

        [Fact]
        public async Task GetByDateRangeAsync_ShouldReturnMovementsWithinRange()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today.AddDays(-1);

            var movements = new[]
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Today.AddDays(-10),
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 30,
                    MovementDate = DateTime.Today.AddDays(-3),
                    StockAfterMovement = 80
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Today,
                    StockAfterMovement = 70
                },
            };

            await _context.StockMovements.AddRangeAsync(movements);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByDateRangeAsync(startDate, endDate);

            result.Should().HaveCount(1);
            result.First().Quantity.Should().Be(30);
        }

        [Fact]
        public async Task GetByMovementTypeAsync_ShouldReturnOnlySpecifiedType()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movements = new[]
            {
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 40
                },
                new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 20,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 60
                }
            };

            await _context.StockMovements.AddRangeAsync(movements);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByMovementTypeAsync(MovementType.IN);

            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.MovementType == MovementType.IN);
        }

        [Fact]
        public async Task SearchMovementsAsync_WithMultipleFilters_ShouldApplyAllFilters()
        {
            var product1 = new Product
            {
                Name = "Product 1",
                SKU = "P1",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5
            };

            var product2 = new Product
            {
                Name = "Product 2",
                SKU = "P2",
                Category = "Test",
                UnitPrice = 20m,
                LowStockThreshold = 5
            };

            await _context.Products.AddRangeAsync(product1, product2);
            await _context.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-5);
            var endDate = DateTime.Today;

            var movements = new[]
            {
                new StockMovement
                {
                    ProductId = product1.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Today.AddDays(-3),
                    StockAfterMovement = 50
                },
                new StockMovement
                {
                    ProductId = product1.ProductId,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Today.AddDays(-2),
                    StockAfterMovement = 40
                },
                new StockMovement
                {
                    ProductId = product2.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = 30,
                    MovementDate = DateTime.Today.AddDays(-1),
                    StockAfterMovement = 30
                }
            };

            await _context.StockMovements.AddRangeAsync(movements);
            await _context.SaveChangesAsync();

            var result = await _repository.SearchMovementsAsync(
                productId: product1.ProductId,
                movementType: MovementType.IN,
                startDate: startDate,
                endDate: endDate
               );

            result.Should().HaveCount(1);
            result.First().ProductId.Should().Be(product1.ProductId);
            result.First().MovementType.Should().Be(MovementType.IN);
        }

        [Fact]
        public async Task GetRecentMovementsAsync_ShouldReturnLimitedOrderedResults()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                LowStockThreshold = 5,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var baseDate = DateTime.Now;

            for (int i = 0; i < 15; i++)
            {
                var movement = new StockMovement
                {
                    ProductId = product.ProductId,
                    MovementType = MovementType.IN,
                    Quantity = i + 1,
                    MovementDate = baseDate.AddMinutes(-i),
                    StockAfterMovement = i + 1
                };
                await _context.StockMovements.AddAsync(movement);
            }
            await _context.SaveChangesAsync();

            var result = await _repository.GetRecentMovementsAsync(10);

            result.Should().HaveCount(10);

            var movementDates = result.Select(m => m.MovementDate).ToList();
            movementDates.Should().BeInDescendingOrder();

            result.First().MovementDate.Should().BeCloseTo(baseDate, TimeSpan.FromSeconds(1));

            result.Last().MovementDate.Should().BeCloseTo(baseDate.AddMinutes(-9), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_StockIN_ShouldIncreaseStock()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 30,
                MovementDate = DateTime.Now,
                Reference = "PO-12345"
            };

            var result = await _repository.AddMovementWithStockUpdateAsync(movement, product);

            result.Should().NotBeNull();
            result.StockAfterMovement.Should().Be(80);

            var updatedProduct = await _context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(80);
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_StockOUT_ShouldDecreaseStock()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.OUT,
                Quantity = 20,
                MovementDate = DateTime.Now,
                Reference = "Sale"
            };

            var result = await _repository.AddMovementWithStockUpdateAsync(movement, product);

            result.Should().NotBeNull();
            result.StockAfterMovement.Should().Be(30);

            var updatedProduct = await _context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(30);
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_StockOUTExceedsAvailable_ShouldThrowException()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 20,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.OUT,
                Quantity = 30,
                MovementDate = DateTime.Now,
                Reason = "Sale"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _repository.AddMovementWithStockUpdateAsync(movement, product));

            var unchangedProduct = await _context.Products.FindAsync(product.ProductId);
            unchangedProduct!.CurrentStock.Should().Be(20);

            var movements = await _repository.GetByProductIdAsync(product.ProductId);
            movements.Should().BeEmpty();
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_ShouldBeAtomic()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 25,
                MovementDate = DateTime.Now
            };

            await _repository.AddMovementWithStockUpdateAsync(movement, product);

            var savedMovement = await _context.StockMovements.FindAsync(movement.MovementId);
            savedMovement.Should().NotBeNull();
            savedMovement!.StockAfterMovement.Should().Be(75);

            var updatedProduct = await _context.Products.FindAsync(product.ProductId);
            updatedProduct!.CurrentStock.Should().Be(75);
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_ShouldSetCreatedDate()
        {
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 10,
                MovementDate = DateTime.Now
            };

            var result = await _repository.AddMovementWithStockUpdateAsync(movement, product);

            result.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task AddMovementWithStockUpdateAsync_ShouldUpdateProductModifiedDate()
        {
            var originalModifiedDate = DateTime.Now.AddDays(-5);
            var product = new Product
            {
                Name = "Test Product",
                SKU = "TEST-001",
                Category = "Test",
                UnitPrice = 10m,
                CurrentStock = 50,
                LowStockThreshold = 10,
                CreatedDate = DateTime.Now.AddDays(-10),
                ModifiedDate = originalModifiedDate,
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var movement = new StockMovement
            {
                ProductId = product.ProductId,
                MovementType = MovementType.IN,
                Quantity = 10,
                MovementDate = DateTime.Now,
            };

            await _repository.AddMovementWithStockUpdateAsync(movement, product);

            var updatedProduct = await _context.Products.FindAsync(product.ProductId);
            updatedProduct!.ModifiedDate.Should().BeAfter(originalModifiedDate);
            updatedProduct.ModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }
    }
}
