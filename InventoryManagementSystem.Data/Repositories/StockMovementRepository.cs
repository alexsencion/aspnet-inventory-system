using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public class StockMovementRepository: Repository<StockMovement>, IStockMovementRepository
    {
        public StockMovementRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StockMovement> AddMovementWithStockUpdateAsync(StockMovement movement, Product product)
        {
            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int newStock;
                    if (movement.MovementType == MovementType.IN)
                    {
                        newStock = product.CurrentStock + movement.Quantity;
                    }
                    else
                    {
                        newStock = product.CurrentStock - movement.Quantity;

                        if (newStock < 0)
                        {
                            throw new InvalidOperationException(
                                $"Insufficient stock. Current stock: {product.CurrentStock}, " +
                                $"Requested: {movement.Quantity}");
                        }
                    }

                    movement.StockAfterMovement = newStock;
                    movement.CreatedDate = DateTime.Now;

                    await _dbSet.AddAsync(movement);

                    product.CurrentStock = newStock;
                    product.ModifiedDate = DateTime.Now;
                    _context.Products.Update(product);

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    var createdMovement = await _dbSet
                        .Include(sm => sm.Product)
                            .ThenInclude(p => p.Supplier)
                        .FirstOrDefaultAsync(sm => sm.MovementId == movement.MovementId);

                    return createdMovement!;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<StockMovement>> GetAllWithProductAsync()
        {
            return await _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Supplier)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet 
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Supplier)
                .Where(sm => sm.MovementDate >= startDate && sm.MovementDate <= endDate)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByMovementTypeAsync(MovementType movementType)
        {
            return await _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Supplier)
                .Where(sm => sm.MovementType == movementType)
                .OrderByDescending (sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
        {
            return await _dbSet 
                .Include(sm => sm.Product)
                .Where(sm  => sm.ProductId == productId)
                .OrderByDescending (sm => sm.MovementDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetRecentMovementsAsync(int count = 10)
        {
            return await _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Supplier)
                .OrderByDescending(sm => sm.MovementDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockMovement>> SearchMovementsAsync(int? productId = null, MovementType? movementType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(sm => sm.Product)
                    .ThenInclude(p => p.Supplier)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(sm => sm.ProductId == productId.Value);
            }

            if (movementType.HasValue)
            {
                query = query.Where(sm => sm.MovementType == movementType.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(sm => sm.MovementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(sm => sm.MovementDate <= endOfDay);
            }

            return await query
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
        }
    }
}
