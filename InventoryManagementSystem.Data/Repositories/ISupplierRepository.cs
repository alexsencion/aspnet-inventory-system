using InventoryManagementSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<IEnumerable<Supplier>> SearchByNameAsync(string searchTerm);
        Task<Supplier?> GetSupplierWithProductsAsync(int supplierId);
        Task<bool> HasProductAsync(int supplierId);
    }
}
