using InventoryManagementSystem.Services.DTOs;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Web.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            var suppliers = string.IsNullOrWhiteSpace(searchTerm)
                ? await _supplierService.GetAllSuppliersAsync()
                : await _supplierService.SearchSupplierAsync(searchTerm);

            var viewModels = suppliers.Select(MapToViewModel).ToList();
            ViewBag.SearchTerm = searchTerm;

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null)
                return NotFound();

            return View(MapToViewModel(supplier));

        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                var createdDto = new CreateSupplierDto
                {
                    Name = viewModel.Name,
                    ContactPerson = viewModel.ContactPerson,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Address = viewModel.Address,
                    Notes = viewModel.Notes
                };

                await _supplierService.CreateSupplierAsync(createdDto);
                TempData["SuccessMessage"] = "Supplier created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", $"Error creating supplier: {ex.Message}");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);
            if (supplier == null) return NotFound();

            return View(MapToViewModel(supplier));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierViewModel viewModel)
        {
            if (id != viewModel.SupplierId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(viewModel);

            try
            {
                var updateDto = new UpdateSupplierDto
                {
                    SupplierId = viewModel.SupplierId,
                    Name = viewModel.Name,
                    ContactPerson = viewModel.ContactPerson,
                    Email = viewModel.Email,
                    Phone = viewModel.Phone,
                    Address = viewModel.Address,
                    Notes = viewModel.Notes
                };

                await _supplierService.UpdateSupplierAsync(updateDto);
                TempData["SuccessMessage"] = "Supplier updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating supplier: {ex.Message}");
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(id);

            if (supplier == null) return NotFound();

            var canDelete = await _supplierService.CanDeleteSupplierAsync(id);
            ViewBag.CanDelete = canDelete;
            ViewBag.DeleteWarning = canDelete
                ? null
                : "This supplier cannot be deleted because it has associated products.";

            return View(MapToViewModel(supplier));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _supplierService.DeleteSupplierAsync(id);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["SuccessMessage"] = "Supplier deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting supplier: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private SupplierViewModel MapToViewModel(SupplierDto dto)
        {
            return new SupplierViewModel
            {
                SupplierId = dto.SupplierId,
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Notes = dto.Notes,
                CreatedDate = dto.CreatedDate,
                ModifiedDate = dto.ModifiedDate,
                ProductCount = dto.ProductCount
            };
        }
    }
}
