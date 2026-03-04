using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public interface IStockMovementRepository : IRepository<StockMovement>
    {
        Task<IEnumerable<StockMovement>> GetAllWithProductAsync();
        Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
        Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(MovementType movementType);
        Task<IEnumerable<StockMovement>> SearchMovementsAsync(
            int? productId = null,
            MovementType? movementType = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<IEnumerable<StockMovement>> GetRecentMovementsAsync(int count = 10);
        Task<StockMovement> AddMovementWithStockUpdateAsync(StockMovement movement, Product product);

    }
}
