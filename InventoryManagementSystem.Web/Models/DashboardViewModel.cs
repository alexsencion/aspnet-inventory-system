namespace InventoryManagementSystem.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalSuppliers { get; set; }
        public decimal TotalInventoryValue { get; set; }

        public IEnumerable<StockMovementViewModel> RecentMovements { get; set; } = new List<StockMovementViewModel>();

        public IEnumerable<ProductViewModel> LowStockItems { get; set; } = new List<ProductViewModel>();

        public int TodayStockIn { get; set; }
        public int TodayStockOut { get; set; }
        public int ThisWeekMovements { get; set; }

        public Dictionary<string, int> ProductsByCategory { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> ValueByCategory { get; set; } = new Dictionary<string, decimal>();
    }

    public class StockLevelsReportViewModel
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        public decimal TotalValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockCount { get; set; }
    }

    public class LowStockReportViewModel
    {
        public IEnumerable<ProductViewModel> LowStockProducts { get; set; } = new List<ProductViewModel>();
        public int TotalLowStockItems { get; set; }
        public decimal ValueAtRisk { get; set; }
    }

    public class MovementHistoryReportViewModel
    {
        public IEnumerable<StockMovementViewModel> Movements { get; set; } = new List<StockMovementViewModel>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalStockIn { get; set; }
        public int TotalStockOut { get; set; }
        public int NetChange { get; set; }
    }
}