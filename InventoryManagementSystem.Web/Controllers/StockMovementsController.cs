using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventoryManagementSystem.Web.Controllers
{
    public class StockMovementsController : Controller
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly IProductService _productService;

        public StockMovementsController(IStockMovementService stockMovementService, IProductService productService)
        {
            _stockMovementService = stockMovementService;
            _productService = productService;
        }

        public async Task<IActionResult> Index(
            int? productId,
            MovementType? movementType,
            DateTime? startDate,
            DateTime? endDate)
        {
            var filterDto = new StockMovementFilterDto
            {
                ProductId = productId,
                MovementType = movementType,
                StartDate = startDate,
                EndDate = endDate
            };

            var movements = await _stockMovementService.SearchMovementsAsync(filterDto);
            var viewModels = movements.Select(MapToViewModel).ToList();

            var filterViewModel = new StockMovementFilterViewModel
            {
                ProductId = productId,
                MovementType = movementType,
                StartDate = startDate,
                EndDate = endDate,
                Products = await GetProductSelectList(),
                MovementTypes = GetMovementTypeSelectList()
            };

            ViewBag.Filter = filterViewModel;
            ViewBag.HasFilters = productId.HasValue || movementType.HasValue || startDate.HasValue || endDate.HasValue; 

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var movement = await _stockMovementService.GetMovementByIdAsync(id);

            if (movement == null)
                return NotFound();

            return View(MapToViewModel(movement));
        }

        public async Task<IActionResult> CreateIn()
        {
            var viewModel = new CreateStockInViewModel()
            {
                Products = await GetProductSelectList(),
                MovementDate = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIn(CreateStockInViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Products = await GetProductSelectList();
                return View(viewModel);
            }

            try
            {
                var createDto = new CreateStockInDto
                {
                    ProductId = viewModel.ProductId,
                    Quantity = viewModel.Quantity,
                    MovementDate = viewModel.MovementDate,
                    Reference = viewModel.Reference,
                    Notes = viewModel.Notes,
                };

                await _stockMovementService.CreateStockInAsync(createDto);
                TempData["SuccessMessage"] = "Stock IN movement recorded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                viewModel.Products = await GetProductSelectList();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error recording stock movement: {ex.Message}");
                viewModel.Products = await GetProductSelectList();
                return View(viewModel);
            }
        }

        public async Task<IActionResult> CreateOut()
        {
            var viewModel = new CreateStockOutViewModel
            {
                Products = await GetProductSelectList(),
                Reasons = GetReasonSelectList(),
                MovementDate = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOut(CreateStockOutViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Products = await GetProductSelectList();
                viewModel.Reasons = GetReasonSelectList();
                return View(viewModel);
            }

            try
            {
                var createDto = new CreateStockOutDto
                {
                    ProductId = viewModel.ProductId,
                    Quantity = viewModel.Quantity,
                    MovementDate = viewModel.MovementDate,
                    Reason = viewModel.Reason,
                    Notes = viewModel.Notes
                };

                await _stockMovementService.CreateStockOutAsync(createDto);
                TempData["SuccessMessage"] = "Stock OUT movement recorded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                viewModel.Products = await GetProductSelectList();
                viewModel.Reasons = GetReasonSelectList();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error recording stock movement: {ex.Message}");
                viewModel.Products = await GetProductSelectList();
                viewModel.Reasons = GetReasonSelectList();
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStock(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return NotFound();

            return Json(new
            {
                productName = product.Name,
                sku = product.SKU,
                currentStock = product.CurrentStock,
                lowStockThreshold = product.LowStockThreshold,
                isLowStock = product.IsLowStock
            });
        }

        private StockMovementViewModel MapToViewModel(StockMovementDto dto)
        {
            return new StockMovementViewModel
            {
                MovementId = dto.MovementId,
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                ProductSKU = dto.ProductSKU,
                MovementType = dto.MovementType,
                Quantity = dto.Quantity,
                MovementDate = dto.MovementDate,
                Reference = dto.Reference,
                Reason = dto.Reason,
                Notes = dto.Notes,
                StockAfterMovement = dto.StockAfterMovement,
                CreatedDate = dto.CreatedDate
            };
        }

        private async Task<IEnumerable<SelectListItem>> GetProductSelectList()
        {
            var products = await _productService.GetAllProductsAsync();
            var selectList = products
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = $"{p.Name} (SKU: {p.SKU}) - Stock: {p.CurrentStock}"
                }).ToList();

            selectList.Insert(0, new SelectListItem { Value = "", Text = "-- Select Product --" });
            return selectList;
        }

        private IEnumerable<SelectListItem> GetMovementTypeSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- All Types --" },
                new SelectListItem { Value = MovementType.IN.ToString(), Text = "-- Stock IN --" },
                new SelectListItem { Value = MovementType.OUT.ToString(), Text = "-- Stock OUT --" },
            };
        }

        private IEnumerable<SelectListItem> GetReasonSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Reason  --" },
                new SelectListItem { Value = "Sale", Text = "Sale" },
                new SelectListItem { Value = "Damage", Text = "Damage" },
                new SelectListItem { Value = "Loss", Text = "Loss" },
                new SelectListItem { Value = "Return", Text = "Return" },
                new SelectListItem { Value = "Transfer", Text = "Transfer" },
                new SelectListItem { Value = "Adjustment", Text = "Adjustment" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
        }
    }
}
