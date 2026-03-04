using InventoryManagementSystem.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Web.Models
{
    public class StockMovementViewModel
    {
        public int MovementId { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "SKU")]
        public string ProductSKU { get; set; } = string.Empty;

        [Display(Name = "Movement Type")]
        public MovementType MovementType { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Movement Date")]
        public DateTime MovementDate { get; set; }

        public string? Reference { get; set; }

        public string? Reason { get; set; }
        public string? Notes { get; set; }

        [Display(Name = "Stock After Movement")]
        public int StockAfterMovement { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
    }

    public class CreateStockInViewModel
    {

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement date is required")]
        [Display(Name = "Movement Date")]
        [DataType(DataType.DateTime)]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
        [Display(Name = "Reference/PO Number")]
        public string? Reference { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem>? Products { get; set; }

        [Display(Name = "Selected Product")]
        public string? SelectedProductName { get; set; }

        [Display(Name = "Current Stock")]
        public int? CurrentStock { get; set; }
    }

    public class CreateStockOutViewModel
    {

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Movement date is required")]
        [Display(Name = "Movement Date")]
        [DataType(DataType.DateTime)]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Reason cannot exceed 50 characters")]
        [Display(Name = "Reason/PO Number")]
        public string? Reason { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem>? Products { get; set; }
        public IEnumerable<SelectListItem>? Reasons { get; set; }

        [Display(Name = "Selected Product")]
        public string? SelectedProductName { get; set; }

        [Display(Name = "Available Stock")]
        public int? AvailableStock { get; set; }
    }

    public class StockMovementFilterViewModel
    {
        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [Display(Name = "Movement Type")]
        public MovementType? MovementType { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public IEnumerable<SelectListItem>? Products { get; set; }
        public IEnumerable<SelectListItem>? MovementTypes { get; set; }
    }
}
