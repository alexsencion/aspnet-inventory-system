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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<bool> CanDeleteProductAsync(int productId)
        {
            return !await _productRepository.HasStockMovementsAsync(productId);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            if (!await _productRepository.IsSkuUniqueAsync(createDto.SKU))
                throw new InvalidOperationException($"SKU '{createDto.SKU}' already exists");

            var product = new Product
            {
                Name = createDto.Name,
                SKU = createDto.SKU,
                Description = createDto.Description,
                Category = createDto.Category,
                UnitPrice = createDto.UnitPrice,
                LowStockThreshold = createDto.LowStockThreshold,
                SupplierId = createDto.SupplierId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
            await _productRepository.AddAsync(product);
            return MapToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
                return false;

            if (await _productRepository.HasStockMovementsAsync(productId))
                throw new InvalidOperationException("Cannot delete product with stock movements history");

            await _productRepository.DeleteAsync(product);
            return true;

        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _productRepository.GetAllCategoriesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithSuppliersAsync();
            return products.Select(MapToDto);
            
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await _productRepository.GetLowStockProductsAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithSupplierAsync(productId);
            return product != null ? MapToDto(product) : null;
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludedProductId = null)
        {
            return await _productRepository.IsSkuUniqueAsync(sku, excludedProductId);
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _productRepository.ExistsAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm, string? category = null)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm, category);
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto)
        {
            var product = await _productRepository.GetByIdAsync(updateDto.ProductId);

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {updateDto.ProductId} not found");

            if (!await _productRepository.IsSkuUniqueAsync(updateDto.SKU, updateDto.ProductId))
            {
                throw new InvalidOperationException($"SKU '{updateDto.SKU}' already exists");
            }

            product.Name = updateDto.Name;
            product.SKU = updateDto.SKU;
            product.Description = updateDto.Description;
            product.Category = updateDto.Category;
            product.UnitPrice = updateDto.UnitPrice;
            product.LowStockThreshold = updateDto.LowStockThreshold;
            product.SupplierId = updateDto.SupplierId;
            product.ModifiedDate = DateTime.Now;

            await _productRepository.UpdateAsync(product);

            var updatedProduct = await _productRepository.GetByIdWithSupplierAsync(product.ProductId);
            return MapToDto(updatedProduct!);
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Category = product.Category,
                UnitPrice = product.UnitPrice,
                CurrentStock = product.CurrentStock,
                LowStockThreshold = product.LowStockThreshold,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                CreatedDate = product.CreatedDate,
                ModifiedDate = product.ModifiedDate,
                IsLowStock = product.IsLowStock,
                InventoryValue = product.InventoryValue,
            };
        }
    }
}
