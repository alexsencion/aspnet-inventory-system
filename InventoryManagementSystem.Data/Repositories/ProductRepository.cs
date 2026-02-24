using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) 
        {
        }
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _dbSet
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllWithSuppliersAsync()
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithSupplierAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Include(p => p.StockMovements)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _dbSet
                .Include (p => p.Supplier)
                .Where(p => p.CurrentStock <= p.LowStockThreshold)
                .OrderBy (p => p.CurrentStock)
                .ToListAsync();
        }

        public async Task<bool> HasStockMovementsAsync(int productId)
        {
            return await _context.StockMovements.AnyAsync(sm => sm.ProductId == productId);
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
        {
            if (excludeProductId.HasValue)
            {
                return !await _dbSet.AnyAsync(p => p.SKU == sku && p.ProductId != excludeProductId.Value);
            }

            return !await _dbSet.AnyAsync(p => p.SKU == sku);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null)
        {
            var query = _dbSet.Include(p => p.Supplier).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => 
                    p.Name.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm) ||
                    p.Description != null && p.Description.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            return await query.OrderBy(p => p.Name).ToListAsync();
        }
    }
}
