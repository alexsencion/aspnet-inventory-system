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
    public class StockMovementServiceTests
    {
        private readonly Mock<IStockMovementRepository> _mockMovementRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly StockMovementService _service;

        public StockMovementServiceTests()
        {
            _mockMovementRepository = new Mock<IStockMovementRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _service = new StockMovementService(_mockMovementRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task GetAllMovementsAsync_ShouldReturnAllMovements()
        {
            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    MovementId = 1,
                    ProductId = 1,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 50,
                    Product = new Product { Name = "Product A", SKU = "PA-001" }
                },

                new StockMovement
                {
                    MovementId = 2,
                    ProductId = 1,
                    MovementType = MovementType.OUT,
                    Quantity = 10,
                    MovementDate = DateTime.Now,
                    StockAfterMovement = 40,
                    Product = new Product { Name = "Product A", SKU = "PA-001" }
                }
            };

            _mockMovementRepository.Setup(r => r.GetAllWithProductAsync())
                .ReturnsAsync(movements);

            var result = await _service.GetAllMovementsAsync();

            result.Should().HaveCount(2);
            result.Should().Contain(m => m.MovementType == MovementType.IN);
            result.Should().Contain(m => m.MovementType == MovementType.OUT);
        }

        [Fact]
        public async Task CreateStockInAsync_WithValidData_ShouldCreateMovement()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "TEST-001",
                CurrentStock = 50,
                LowStockThreshold = 10,
            };

            var createdDto = new CreateStockInDto
            {
                ProductId = 1,
                Quantity = 30,
                MovementDate = DateTime.Now,
                Reference = "PO-12345",
                Notes = "Test notes"
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var createdMovement = new StockMovement
            {
                MovementId = 1,
                ProductId = 1,
                MovementType = MovementType.IN,
                Quantity = 30,
                MovementDate = createdDto.MovementDate,
                Reference = "PO-12345",
                Notes = "Test notes",
                StockAfterMovement = 80,
                Product = product
            };

            _mockMovementRepository.Setup(r => r.AddMovementWithStockUpdateAsync(
                It.IsAny<StockMovement>(), product))
                .ReturnsAsync(createdMovement);

            var result = await _service.CreateStockInAsync(createdDto);

            result.Should().NotBeNull();
            result.MovementType.Should().Be(MovementType.IN);
            result.Quantity.Should().Be(30);
            result.StockAfterMovement.Should().Be(80);
            result.Reference.Should().Be("PO-12345");
        }

        [Fact]
        public async Task CreateStockInAsync_WhenProductNotFound_ShouldThrowException()
        {
            var createdDto = new CreateStockInDto
            {
                ProductId = 999,
                Quantity = 30,
                MovementDate = DateTime.Now,
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateStockInAsync(createdDto));
        }

        [Fact]
        public async Task CreateStockInAsync_WithFutureDate_ShouldThrowException()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                CurrentStock = 50
            };

            var createDto = new CreateStockInDto
            {
                ProductId = 1,
                Quantity = 30,
                MovementDate = DateTime.Now.AddDays(1)
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateStockInAsync(createDto));
        }

        [Fact]
        public async Task CreateStockOutAsync_WithValidData_ShouldCreateMovement()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "TEST-001",
                CurrentStock = 50,
                LowStockThreshold = 10
            };

            var createDto = new CreateStockOutDto
            {
                ProductId = 1,
                Quantity = 20,
                MovementDate = DateTime.Now,
                Reason = "Sale",
                Notes = "Test sale"
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var createdMovement = new StockMovement
            {
                MovementId = 1,
                ProductId = 1,
                MovementType = MovementType.OUT,
                Quantity = 20,
                MovementDate = createDto.MovementDate,
                Reason = "Sale",
                Notes = "Test sale",
                StockAfterMovement = 30,
                Product = product
            };

            _mockMovementRepository.Setup(r => r.AddMovementWithStockUpdateAsync(
                It.IsAny<StockMovement>(), product))
                .ReturnsAsync(createdMovement);

            var result = await _service.CreateStockOutAsync(createDto);

            result.Should().NotBeNull();
            result.MovementType.Should().Be(MovementType.OUT);
            result.Quantity.Should().Be(20);
            result.StockAfterMovement.Should().Be(30);
            result.Reason.Should().Be("Sale");
        }

        [Fact]
        public async Task CreateStockOutAsync_WithInsufficientStock_ShouldThrowException()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                CurrentStock = 10
            };

            var createDto = new CreateStockOutDto
            {
                ProductId = 1,
                Quantity = 20,
                MovementDate = DateTime.Now,
                Reason = "Sale"
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateStockOutAsync(createDto));

            exception.Message.Should().Contain("Insufficient stock");
        }

        [Fact]
        public async Task CreateStockOutAsync_WhenProductNotFound_ShouldThrowException()
        {
            var createDto = new CreateStockOutDto
            {
                ProductId = 999,
                Quantity = 10,
                MovementDate = DateTime.Now,
                Reason = "Sale"
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CreateStockOutAsync(createDto));
        }

        [Fact]
        public async Task CreateStockOutAsync_WithFutureDate_ShouldThrowException()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                CurrentStock = 50
            };

            var createDto = new CreateStockOutDto
            {
                ProductId = 1,
                Quantity = 10,
                MovementDate = DateTime.Now.AddDays(1),
                Reason = "Sale"
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateStockOutAsync(createDto));
        }

        [Fact]
        public async Task GetMovementsByProductIdAsync_ShouldReturnProductMovements()
        {
            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    MovementId = 1,
                    ProductId = 1,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    StockAfterMovement = 50,
                    Product = new Product { Name = "Product A", SKU = "PA-001" }
                }
            };

            _mockMovementRepository.Setup(r => r.GetByProductIdAsync(1))
                .ReturnsAsync(movements);

            var result = await _service.GetMovementsByProductIdAsync(1);

            result.Should().HaveCount(1);
            result.First().ProductId.Should().Be(1);
        }

        [Fact]
        public async Task SearchMovementsAsync_ShouldApplyFilters()
        {
            var filter = new StockMovementFilterDto
            {
                ProductId = 1,
                MovementType = MovementType.IN,
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today
            };

            var movements = new List<StockMovement>
            {
                new StockMovement
                {
                    MovementId = 1,
                    ProductId = 1,
                    MovementType = MovementType.IN,
                    Quantity = 50,
                    MovementDate = DateTime.Today.AddDays(-3),
                    StockAfterMovement = 50,
                    Product = new Product { Name = "Product A", SKU = "PA-001" }
                }
            };

            _mockMovementRepository.Setup(r => r.SearchMovementsAsync(
                1, MovementType.IN, filter.StartDate, filter.EndDate))
                .ReturnsAsync(movements);

            var result = await _service.SearchMovementsAsync(filter);

            result.Should().HaveCount(1);
            result.First().ProductId.Should().Be(1);
            result.First().MovementType.Should().Be(MovementType.IN);
        }

        [Fact]
        public async Task GetRecentMovementsAsync_ShouldReturnLimitedResults()
        {
            var movements = Enumerable.Range(1, 5).Select(i => new StockMovement
            {
                MovementId = i,
                ProductId = 1,
                MovementType = MovementType.IN,
                Quantity = i * 10,
                StockAfterMovement= i * 10,
                Product = new Product { Name = "Product A", SKU = "PA-001" }
            }).ToList();

            _mockMovementRepository.Setup(r => r.GetRecentMovementsAsync(5))
                .ReturnsAsync(movements);

            var result = await _service.GetRecentMovementsAsync(5);

            result.Should().HaveCount(5);
        }

        [Fact]
        public async Task CanCreateStockOutAsync_WithSufficientStock_ShouldReturnTrue()
        {
            var product = new Product
            {
                ProductId = 1,
                CurrentStock = 50
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.CanCreateStockOutAsync(1, 50);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanCreateStockOutAsync_WithInSufficientStock_ShouldReturnFalse()
        {
            var product = new Product
            {
                ProductId = 1,
                CurrentStock = 20
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.CanCreateStockOutAsync(1, 30);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanCreateStockOutAsync_WithProductNotFound_ShouldReturnFalse()
        {
            _mockProductRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            var result = await _service.CanCreateStockOutAsync(999, 10);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAvailableStockAsync_WhenProductExists_ShouldReturnStock()
        {
            var product = new Product
            {
                ProductId = 1,
                CurrentStock = 75
            };

            _mockProductRepository.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.GetAvailableStockAsync(1);

            result.Should().Be(75);
        }

        [Fact]
        public async Task GetAvailableStockAsync_WhenProductNotFound_ShouldReturnZero()
        {
            _mockProductRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Product?)null);

            var result = await _service.GetAvailableStockAsync(999);

            result.Should().Be(0);
        }
    }
}
