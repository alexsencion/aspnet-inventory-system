using System.Diagnostics;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IStockMovementService _stockMovementService;

        public HomeController(IProductService productService, ISupplierService supplierService, IStockMovementService stockMovementService)
        {
            _productService = productService;
            _supplierService = supplierService;
            _stockMovementService = stockMovementService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var recentMovements = await _stockMovementService.GetRecentMovementsAsync(10);
            var lowStockProducts = await _productService.GetLowStockProductsAsync();

            var todayStart = DateTime.Today;
            var todayEnd = DateTime.Today.AddDays(1).AddTicks(-1);
            var todayMovements = await _stockMovementService.SearchMovementsAsync(new StockMovementFilterDto
            {
                StartDate = todayStart,
                EndDate = todayEnd
            });

            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var thisWeekMovements = await _stockMovementService.SearchMovementsAsync(new StockMovementFilterDto
            {
                StartDate = weekStart,
                EndDate = DateTime.Now
            });

            // Category breakdown
            var productsByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var valueByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.InventoryValue));

            var viewModel = new DashboardViewModel
            {
                TotalProducts = products.Count(),
                LowStockProducts = lowStockProducts.Count(),
                TotalSuppliers = suppliers.Count(),
                TotalInventoryValue = products.Sum(p => p.InventoryValue),
                RecentMovements = recentMovements.Select(MapMovementToViewModel),
                LowStockItems = lowStockProducts.Take(5).Select(MapProductToViewModel),
                TodayStockIn = todayMovements.Count(m => m.MovementType == Data.Entities.MovementType.IN),
                TodayStockOut = todayMovements.Count(m => m.MovementType == Data.Entities.MovementType.OUT),
                ThisWeekMovements = thisWeekMovements.Count(),
                ProductsByCategory = productsByCategory,
                ValueByCategory = valueByCategory
            };

            return View(viewModel);
        }

        public async Task<IActionResult> StockLevelsReport()
        {
            var products = await _productService.GetAllProductsAsync();

            var viewModel = new StockLevelsReportViewModel
            {
                Products = products.Select(MapProductToViewModel).OrderBy(p => p.Name),
                TotalValue = products.Sum(p => p.InventoryValue),
                TotalItems = products.Sum(p => p.CurrentStock),
                LowStockCount = products.Count(p => p.IsLowStock),
            };

            return View(viewModel);
        }

        public async Task<IActionResult> LowStockReport()
        {
            var lowStockProducts = await _productService.GetLowStockProductsAsync();

            var viewModel = new LowStockReportViewModel
            {
                LowStockProducts = lowStockProducts.Select(MapProductToViewModel).OrderBy(p => p.CurrentStock),
                TotalLowStockItems = lowStockProducts.Count(),
                ValueAtRisk = lowStockProducts.Sum(p => p.InventoryValue)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> MovementHistoryReport(DateTime? startDate,  DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var movements = await _stockMovementService.SearchMovementsAsync(new StockMovementFilterDto
            {
                StartDate = startDate,
                EndDate = endDate
            });

            var movementsList = movements.ToList();

            var viewModel = new MovementHistoryReportViewModel
            {
                Movements = movementsList.Select(MapMovementToViewModel),
                StartDate = startDate,
                EndDate = endDate,
                TotalStockIn = movementsList.Where(m => m.MovementType == Data.Entities.MovementType.IN).Sum(m => m.Quantity),
                TotalStockOut = movementsList.Where(m => m.MovementType == Data.Entities.MovementType.OUT).Sum(m => m.Quantity),
                NetChange = movementsList.Where(m => m.MovementType == Data.Entities.MovementType.IN).Sum(m => m.Quantity) -
                            movementsList.Where(m => m.MovementType == Data.Entities.MovementType.OUT).Sum(m => m.Quantity)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportStockLevels()
        {
            var products = await _productService.GetAllProductsAsync();

            var csv = "Product Name, SKU, Category, Supplier, Unit Price, Current Stock, Low Stock Threshold, Inventory Value, Status\n";

            foreach (var product in products.OrderBy(p => p.Name))
            {
                csv += $"\"{product.Name}\",\"{product.SKU}\",\"{product.Category}\",\"{product.SupplierName ?? "N/A"}\"," +
                       $"{product.UnitPrice},{product.CurrentStock},{product.LowStockThreshold},{product.InventoryValue}," +
                       $"\"{(product.IsLowStock ? "Low Stock" : "Normal")}\"\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"stock-levels-{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> ExportLowStock()
        {
            var products = await _productService.GetLowStockProductsAsync();

            var csv = "Product Name, SKU, Category, Supplier, Current Stock, Low Stock Threshold, Shortfall, Unit Price, Inventory Value\n";

            foreach (var product in products.OrderBy(p => p.CurrentStock))
            {
                var shortfall = product.LowStockThreshold - product.CurrentStock;
                csv += $"\"{product.Name}\",\"{product.SKU}\",\"{product.Category}\",\"{product.SupplierName ?? "N/A"}\"," +
                       $"{product.CurrentStock},{product.LowStockThreshold},{shortfall},{product.UnitPrice},{product.InventoryValue}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"low-stock-{DateTime.Now:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> ExportMovementHistory(DateTime? startDate,  DateTime? endDate)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddDays(-30);

            if (!endDate.HasValue)
                endDate = DateTime.Today;

            var movements = await _stockMovementService.SearchMovementsAsync(new StockMovementFilterDto
            {
                StartDate = startDate,
                EndDate = endDate
            });

            var csv = "Date, Type, Product Name, SKU, Quantity, Reference/Reason, Stock After Movement, Notes\n";

            foreach (var movement in movements.OrderByDescending(m => m.MovementDate))
            {
                var refReason = movement.MovementType == Data.Entities.MovementType.IN
                    ? movement.Reference ?? ""
                    : movement.Reason ?? "";

                csv += $"\"{movement.MovementDate:yyyy-MM-dd HH:mm}\",\"{movement.MovementType}\"," +
                       $"\"{movement.ProductName}\",\"{movement.ProductSKU}\",{movement.Quantity}," +
                       $"\"{refReason}\",{movement.StockAfterMovement},\"{movement.Notes ?? ""}\"\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", 
                $"movement-history-{startDate:yyyyMMdd}---{endDate:yyyyMMdd}.csv");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private ProductViewModel MapProductToViewModel(ProductDto dto)
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

        private StockMovementViewModel MapMovementToViewModel(StockMovementDto dto)
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

    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
