using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;

        public ProductsController(IProductService productService, ISupplierService supplierService)
        {
            _productService = productService;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(string searchTerm, string category, bool? lowStockOnly)
        {
            var products = lowStockOnly == true
                ? await _productService.GetLowStockProductsAsync()
                : string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(category)
                    ? await _productService.GetAllProductsAsync()
                    : await _productService.SearchProductsAsync(searchTerm ?? "", category);

            var viewModels = products.Select(MapToViewModel).ToList();

            ViewBag.Search = searchTerm;
            ViewBag.Category = category;
            ViewBag.LowStockOnly = lowStockOnly;
            ViewBag.Categories = await GetCategorySelectList();

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(MapToViewModel(product));
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductViewModel
            {
                Suppliers = await GetSuppliersSelectList(),
                Categories = await GetCategorySelectList(includeEmpty: true)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }

            try
            {
                var createDto = new CreateProductDto
                {
                    Name = viewModel.Name,
                    SKU = viewModel.SKU,
                    Description = viewModel.Description,
                    Category = viewModel.Category,
                    UnitPrice = viewModel.UnitPrice,
                    LowStockThreshold = viewModel.LowStockThreshold,
                    SupplierId = viewModel.SupplierId,
                };

                await _productService.CreateProductAsync(createDto);
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("SKU", ex.Message);
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            var viewModel = MapToViewModel(product);
            viewModel.Suppliers = await GetSuppliersSelectList();
            viewModel.Categories = await GetCategorySelectList(includeEmpty: true);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel viewModel)
        {
            if (id != viewModel.ProductId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }

            try
            {
                var updateDto = new UpdateProductDto
                {
                    ProductId = viewModel.ProductId,
                    Name = viewModel.Name,
                    SKU = viewModel.SKU,
                    Description = viewModel.Description,
                    Category = viewModel.Category,
                    UnitPrice = viewModel.UnitPrice,
                    LowStockThreshold = viewModel.LowStockThreshold,
                    SupplierId = viewModel.SupplierId
                };

                await _productService.UpdateProductAsync(updateDto);
                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("SKU", ex.Message);
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                viewModel.Suppliers = await GetSuppliersSelectList();
                viewModel.Categories = await GetCategorySelectList(includeEmpty: true);
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            var canDelete = await _productService.CanDeleteProductAsync(id);
            ViewBag.CanDelete = canDelete;
            ViewBag.DeleteWarning = canDelete
                ? null
                : "This product cannot be deleted because it has stock movement history";

            return View(MapToViewModel(product));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private ProductViewModel MapToViewModel(ProductDto dto)
        {
            return new ProductViewModel
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                SKU = dto.SKU,
                Description = dto.Description,
                Category = dto.Category,
                UnitPrice = dto.UnitPrice,
                CurrentStock = dto.CurrentStock,
                LowStockThreshold = dto.LowStockThreshold,
                SupplierId = dto.SupplierId,
                SupplierName = dto.SupplierName,
                CreatedDate = dto.CreatedDate,
                ModifiedDate = dto.ModifiedDate,
                IsLowStock = dto.IsLowStock,
                InventoryValue = dto.InventoryValue
            };
        }
        
        private async Task<IEnumerable<SelectListItem>> GetSuppliersSelectList()
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var selectList = suppliers.Select(s => new SelectListItem
            {
                Value = s.SupplierId.ToString(),
                Text = s.Name
            }).ToList();

            selectList.Insert(0, new SelectListItem { Value = "", Text = "-- Select Supplier (Optional) --" });
            return selectList;
        }
        

        private async Task<IEnumerable<SelectListItem>> GetCategorySelectList(bool includeEmpty = false)
        {
            var categories = await _productService.GetAllCategoriesAsync();
            var selectList = categories.Select(c => new SelectListItem
            {
                Value = c,
                Text = c
            }).ToList();

            if (includeEmpty)
            {
                selectList.Insert(0, new SelectListItem { Value = "", Text = "-- Enter New or Select Existing ---" });
            }
            else
            {
                selectList.Insert(0, new SelectListItem { Value = "", Text = "--- All Categories ---" });
            }

            return selectList;
        }
    }
}
