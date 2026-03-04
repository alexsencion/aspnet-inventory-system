using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.DTOs
{
    public class StockMovementDto
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductSKU { get; set; } = string.Empty;

        public MovementType MovementType{ get; set; }
        public int Quantity { get; set; }

        public DateTime MovementDate { get; set; }

        public string? Reference { get; set; }

        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public int StockAfterMovement { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateStockInDto
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement date is required")]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
        public string? Reference { get; set; }

        [StringLength(100, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public class CreateStockOutDto
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement date is required")]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(50, ErrorMessage = "Reference cannot exceed 50 characters")]
        public string? Reason { get; set; }

        [StringLength(100, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public class StockMovementFilterDto
    {
        public int? ProductId { get; set; }
        public MovementType? MovementType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
