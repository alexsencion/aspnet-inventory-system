using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllWithSuppliersAsync();
        Task<Product?> GetByIdWithSupplierAsync(int productId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, string? category = null);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<bool> HasStockMovementsAsync(int productId);

    }
}
