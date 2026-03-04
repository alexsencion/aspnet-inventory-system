using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Services
{
    public class StockMovementService : IStockMovementService
    {
        private readonly IStockMovementRepository _stockMovementRepository;
        public readonly IProductRepository _productRepository;

        public StockMovementService(IStockMovementRepository stockMovementRepository, IProductRepository productRepository)
        {
            _stockMovementRepository = stockMovementRepository;
            _productRepository = productRepository;
        }
        public async Task<bool> CanCreateStockOutAsync(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return false;

            return product.CurrentStock >= quantity;
        }

        public async Task<StockMovementDto> CreateStockInAsync(CreateStockInDto createDto)
        {
            var product = await _productRepository.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {createDto.ProductId} not found");

            if (createDto.MovementDate > DateTime.Now)
                throw new InvalidOperationException("Movement date cannot be in the future");

            var movement = new StockMovement
            {
                ProductId = createDto.ProductId,
                MovementType = MovementType.IN,
                Quantity = createDto.Quantity,
                MovementDate = createDto.MovementDate,
                Reference = createDto.Reference,
                Notes = createDto.Notes
            };

            var createdMovement = await _stockMovementRepository.AddMovementWithStockUpdateAsync(movement, product);

            return MapToDto(createdMovement);
        }

        public async Task<StockMovementDto> CreateStockOutAsync(CreateStockOutDto createDto)
        {
            var product = await _productRepository.GetByIdAsync(createDto.ProductId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {createDto.ProductId} not found");

            if (createDto.MovementDate > DateTime.Now)
                throw new InvalidOperationException("Movement date cannot be in the future");

            var movement = new StockMovement
            {
                ProductId = createDto.ProductId,
                MovementType = MovementType.OUT,
                Quantity = createDto.Quantity,
                MovementDate = createDto.MovementDate,
                Reference = createDto.Reason,
                Notes = createDto.Notes
            };

            var createdMovement = await _stockMovementRepository.AddMovementWithStockUpdateAsync(movement, product);

            return MapToDto(createdMovement);
        }

        public async Task<IEnumerable<StockMovementDto>> GetAllMovementsAsync()
        {
            var movements = await _stockMovementRepository.GetAllWithProductAsync();
            return movements.Select(MapToDto);
        }

        public async Task<int> GetAvailableStockAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product?.CurrentStock ?? 0;
        }

        public async Task<StockMovementDto?> GetMovementByIdAsync(int movementId)
        {
            var movement = await _stockMovementRepository.GetByIdAsync(movementId);
            if (movement == null)
                return null;

            var movements = await _stockMovementRepository.GetByProductIdAsync(movement.ProductId);
            var movementWithProduct = movements.FirstOrDefault(m => m.MovementId == movementId);

            return movementWithProduct != null ? MapToDto(movementWithProduct) : null;
        }

        public async Task<IEnumerable<StockMovementDto>> GetMovementsByProductIdAsync(int productId)
        {
            var movements = await _stockMovementRepository.GetByProductIdAsync(productId);
            return movements.Select(MapToDto);
        }

        public async Task<IEnumerable<StockMovementDto>> GetRecentMovementsAsync(int count = 10)
        {
            var movement = await _stockMovementRepository.GetRecentMovementsAsync(count);
            return movement.Select(MapToDto);
        }

        public async Task<IEnumerable<StockMovementDto>> SearchMovementsAsync(StockMovementFilterDto filter)
        {
            var movements = await _stockMovementRepository.SearchMovementsAsync(
                productId: filter.ProductId,
                movementType: filter.MovementType,
                startDate: filter.StartDate,
                endDate: filter.EndDate
            );

            return movements.Select(MapToDto);
        }

        private StockMovementDto MapToDto(StockMovement movement)
        {
            return new StockMovementDto
            {
                MovementId = movement.MovementId,
                ProductId = movement.ProductId,
                ProductName = movement.Product?.Name ?? "Unknown Product",
                ProductSKU = movement.Product?.SKU ?? "N/A",
                MovementType = movement.MovementType,
                Quantity = movement.Quantity,
                MovementDate = movement.MovementDate,
                Reference = movement.Reference,
                Reason = movement.Reason,
                Notes = movement.Notes,
                StockAfterMovement = movement.StockAfterMovement,
                CreatedDate = movement.CreatedDate
            };
        }
    }
}
