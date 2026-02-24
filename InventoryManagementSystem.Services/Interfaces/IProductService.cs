using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, string? category =  null);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> ProductExistsAsync(int productId);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludedProductId = null);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<bool> CanDeleteProductAsync(int productId);
    }
}
