using InventoryManagementSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext context) : base(context) 
        {
        }
        public async Task<Supplier?> GetSupplierWithProductsAsync(int supplierId)
        {
            return await _dbSet
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        }

        public async Task<bool> HasProductAsync(int supplierId)
        {
            return await _context.Products.AnyAsync(p => p.SupplierId == supplierId);
        }

        public async Task<IEnumerable<Supplier>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(s => s.Name.Contains(searchTerm) ||
                            s.ContactPerson.Contains(searchTerm) ||
                            s.Email.Contains(searchTerm))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}
