using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Web.Models
{
    public class ProductViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be 0 or greater")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "Low stock threshold is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold must be 0 or greater")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "Supplier Name")]
        public string? SupplierName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Last Modified")]
        public DateTime ModifiedDate { get; set; }

        [Display(Name = "Low Stock")]
        public bool IsLowStock { get; set; }

        [Display(Name = "Inventory Value")]
        [DataType(DataType.Currency)]
        public decimal InventoryValue { get; set; }

        // For Dropdowns
        public IEnumerable<SelectListItem>? Suppliers { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
