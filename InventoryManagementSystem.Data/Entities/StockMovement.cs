using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Entities
{
    public class StockMovement
    {
        [Key]
        public int MovementId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public MovementType MovementType { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Reference { get; set; }

        [MaxLength(50)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockAfterMovement { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

    }

    public enum MovementType
    {
        IN,
        OUT
    }
}
