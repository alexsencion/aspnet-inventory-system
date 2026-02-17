using InventoryManagementSystem.Data.Entities;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<bool> CanDeleteSupplierAsync(int supplierId)
        {
            return !await _supplierRepository.HasProductAsync(supplierId);
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createDto)
        {
            var supplier = new Supplier
            {
                Name = createDto.Name,
                ContactPerson = createDto.ContactPerson,
                Email = createDto.Email,
                Phone = createDto.Phone,
                Address = createDto.Address,
                Notes = createDto.Notes,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            await _supplierRepository.AddAsync(supplier);
            return MapToDto(supplier);

        }

        public async Task<bool> DeleteSupplierAsync(int supplierId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);

            if (supplier == null)
                return false;

            if (await _supplierRepository.HasProductAsync(supplierId))
                throw new InvalidOperationException("Cannot delete supplier with associated products");

            await _supplierRepository.DeleteAsync(supplier);
            return true;
        }


        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Select(MapToDto).OrderBy(s =>  s.Name);
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int supplierId)
        {
            var supplier = await _supplierRepository.GetSupplierWithProductsAsync(supplierId);
            return supplier != null ? MapToDto(supplier) : null;
        }

        public async Task<IEnumerable<SupplierDto>> SearchSupplierAsync(string searchTerm)
        {
            var suppliers = await _supplierRepository.SearchByNameAsync(searchTerm);
            return suppliers.Select(MapToDto);
        }

        public async Task<bool> SupplierExistsAsync(int supplierId)
        {
            return await _supplierRepository.ExistsAsync(s => s.SupplierId == supplierId);
        }

        public async Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierDto updateDto)
        {
            var supplier = await _supplierRepository.GetByIdAsync(updateDto.SupplierId);

            if (supplier == null)
                throw new KeyNotFoundException($"Supplier with ID {updateDto.SupplierId} not found");

            supplier.Name = updateDto.Name;
            supplier.ContactPerson = updateDto.ContactPerson;
            supplier.Email = updateDto.Email;
            supplier.Phone = updateDto.Phone;
            supplier.Address = updateDto.Address;
            supplier.Notes = updateDto.Notes;
            supplier.ModifiedDate = DateTime.Now;

            await _supplierRepository.UpdateAsync(supplier);
            return MapToDto(supplier);

        }

        private SupplierDto MapToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                ContactPerson = supplier.ContactPerson,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                Notes = supplier.Notes,
                CreatedDate = supplier.CreatedDate,
                ModifiedDate = supplier.ModifiedDate,
                ProductCount = supplier.Products?.Count ?? 0
            };
        }
    }
}
