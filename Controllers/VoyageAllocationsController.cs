using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.ViewModels;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Controller for managing VoyageAllocation entities
    /// Demonstrates complex ViewModels with dropdown lists and relationships
    /// Admin-only access for approving voyage allocations
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class VoyageAllocationsController : Controller
    {
        private readonly IVoyageAllocationRepository _allocationRepository;
        private readonly ICargoParcelRepository _parcelRepository;
        private readonly IVesselRepository _vesselRepository;
        private readonly ILogger<VoyageAllocationsController> _logger;

        public VoyageAllocationsController(
            IVoyageAllocationRepository allocationRepository,
            ICargoParcelRepository parcelRepository,
            IVesselRepository vesselRepository,
            ILogger<VoyageAllocationsController> logger)
        {
            _allocationRepository = allocationRepository ?? throw new ArgumentNullException(nameof(allocationRepository));
            _parcelRepository = parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));
            _vesselRepository = vesselRepository ?? throw new ArgumentNullException(nameof(vesselRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: VoyageAllocations
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var allocations = await _allocationRepository.GetAllAllocationsWithDetailsAsync();
                return View(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voyage allocations");
                TempData["Error"] = "An error occurred while loading voyage allocations.";
                return View(new List<VoyageAllocation>());
            }
        }

        // GET: VoyageAllocations/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var allocation = await _allocationRepository.GetAllocationWithDetailsAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = $"Voyage allocation with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving allocation details for ID: {AllocationId}", id);
                TempData["Error"] = "An error occurred while loading allocation details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: VoyageAllocations/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new VoyageAllocationViewModel
            {
                LoadingDate = DateTime.Today,
                DischargeDate = DateTime.Today.AddDays(15),
                FreightRate = 2.50m,
                DemurrageRate = 25000m
            };

            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // POST: VoyageAllocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoyageAllocationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var allocation = new VoyageAllocation
                {
                    ParcelId = viewModel.ParcelId,
                    VesselId = viewModel.VesselId,
                    LoadingDate = viewModel.LoadingDate,
                    DischargeDate = viewModel.DischargeDate,
                    FreightRate = viewModel.FreightRate,
                    DemurrageRate = viewModel.DemurrageRate
                };

                await _allocationRepository.AddAsync(allocation);
                await _allocationRepository.SaveChangesAsync();

                _logger.LogInformation("Created voyage allocation for Parcel ID: {ParcelId}, Vessel ID: {VesselId}",
                    allocation.ParcelId, allocation.VesselId);

                TempData["Success"] = "Voyage allocation created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voyage allocation");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the allocation.");
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        // GET: VoyageAllocations/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var allocation = await _allocationRepository.GetByIdAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = $"Voyage allocation with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new VoyageAllocationViewModel
                {
                    Id = allocation.Id,
                    ParcelId = allocation.ParcelId,
                    VesselId = allocation.VesselId,
                    LoadingDate = allocation.LoadingDate,
                    DischargeDate = allocation.DischargeDate,
                    FreightRate = allocation.FreightRate,
                    DemurrageRate = allocation.DemurrageRate
                };

                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation for edit, ID: {AllocationId}", id);
                TempData["Error"] = "An error occurred while loading the allocation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: VoyageAllocations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VoyageAllocationViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                var allocation = await _allocationRepository.GetByIdAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = $"Voyage allocation with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                allocation.ParcelId = viewModel.ParcelId;
                allocation.VesselId = viewModel.VesselId;
                allocation.LoadingDate = viewModel.LoadingDate;
                allocation.DischargeDate = viewModel.DischargeDate;
                allocation.FreightRate = viewModel.FreightRate;
                allocation.DemurrageRate = viewModel.DemurrageRate;

                await _allocationRepository.UpdateAsync(allocation);
                await _allocationRepository.SaveChangesAsync();

                _logger.LogInformation("Updated voyage allocation ID: {AllocationId}", id);

                TempData["Success"] = "Voyage allocation updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating allocation ID: {AllocationId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the allocation.");
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        // GET: VoyageAllocations/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var allocation = await _allocationRepository.GetAllocationWithDetailsAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = $"Voyage allocation with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation for delete, ID: {AllocationId}", id);
                TempData["Error"] = "An error occurred while loading the allocation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: VoyageAllocations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var allocation = await _allocationRepository.GetByIdAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = $"Voyage allocation with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                await _allocationRepository.DeleteAsync(allocation);
                await _allocationRepository.SaveChangesAsync();

                _logger.LogInformation("Deleted voyage allocation ID: {AllocationId}", id);

                TempData["Success"] = "Voyage allocation deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting allocation ID: {AllocationId}", id);
                TempData["Error"] = "An error occurred while deleting the allocation.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Helper method to populate dropdown lists for parcels and vessels
        /// </summary>
        private async Task PopulateDropdownsAsync(VoyageAllocationViewModel viewModel)
        {
            var parcels = await _parcelRepository.GetAllAsync();
            var vessels = await _vesselRepository.GetAllAsync();

            viewModel.AvailableParcels = parcels.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.ParcelName} - {p.CrudeGrade} ({p.QuantityBbls:N0} bbls)"
            });

            viewModel.AvailableVessels = vessels.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.VesselName} ({v.VesselType}) - {v.CurrentStatus}"
            });
        }
    }
}
