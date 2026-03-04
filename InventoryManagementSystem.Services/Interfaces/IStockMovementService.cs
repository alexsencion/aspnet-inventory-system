using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovementDto>> GetAllMovementsAsync();
        Task<StockMovementDto?> GetMovementByIdAsync(int movementId);
        Task<IEnumerable<StockMovementDto>> GetMovementsByProductIdAsync(int productId);
        Task<IEnumerable<StockMovementDto>> SearchMovementsAsync(StockMovementFilterDto filter);
        Task<IEnumerable<StockMovementDto>> GetRecentMovementsAsync(int count = 10);
        Task<StockMovementDto> CreateStockInAsync(CreateStockInDto createDto);
        Task<StockMovementDto> CreateStockOutAsync(CreateStockOutDto createDto);
        Task<bool> CanCreateStockOutAsync(int productId, int quantity);
        Task<int> GetAvailableStockAsync(int productId);
    }
}
