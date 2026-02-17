using InventoryManagementSystem.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
        Task<SupplierDto?> GetSupplierByIdAsync(int supplierId);
        Task<IEnumerable<SupplierDto>> SearchSupplierAsync(string searchTerm);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto);
        Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierDto updateDto);
        Task<bool> DeleteSupplierAsync(int supplierId);
        Task<bool> SupplierExistsAsync(int supplierId);
        Task<bool> CanDeleteSupplierAsync(int supplierId);
    }
}
