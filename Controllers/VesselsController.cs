using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.ViewModels;
using CargoParcelTracker.Services;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Controller for managing Vessel entities with full CRUD operations
    /// Demonstrates async/await, model validation, caching, and performance monitoring
    /// Admin-only access for vessel management
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class VesselsController : Controller
    {
        private readonly IVesselRepository _vesselRepository;
        private readonly ICacheService _cacheService;
        private readonly IPerformanceMonitoringService _performanceService;
        private readonly ILogger<VesselsController> _logger;

        public VesselsController(
            IVesselRepository vesselRepository,
            ICacheService cacheService,
            IPerformanceMonitoringService performanceService,
            ILogger<VesselsController> logger)
        {
            _vesselRepository = vesselRepository ?? throw new ArgumentNullException(nameof(vesselRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _performanceService = performanceService ?? throw new ArgumentNullException(nameof(performanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Vessels
        [HttpGet]
        public async Task<IActionResult> Index(VesselType? type, VesselStatus? status)
        {
            try
            {
                using (_performanceService.MeasureOperation("Vessels.Index"))
                {
                    IEnumerable<Vessel> vessels;
                    string cacheKey;

                    if (type.HasValue)
                    {
                        cacheKey = CacheKeys.VesselsByStatus(type.Value.ToString());
                        vessels = _cacheService.Get<IEnumerable<Vessel>>(cacheKey);

                        if (vessels == null)
                        {
                            vessels = await _vesselRepository.GetVesselsByTypeAsync(type.Value);
                            _cacheService.Set(cacheKey, vessels, TimeSpan.FromMinutes(5));
                        }

                        ViewBag.FilterType = type.Value.ToString();
                    }
                    else if (status.HasValue)
                    {
                        cacheKey = CacheKeys.VesselsByStatus(status.Value.ToString());
                        vessels = _cacheService.Get<IEnumerable<Vessel>>(cacheKey);

                        if (vessels == null)
                        {
                            vessels = await _vesselRepository.GetVesselsByStatusAsync(status.Value);
                            _cacheService.Set(cacheKey, vessels, TimeSpan.FromMinutes(5));
                        }

                        ViewBag.FilterStatus = status.Value.ToString();
                    }
                    else
                    {
                        // Cache all vessels for 5 minutes
                        vessels = _cacheService.Get<IEnumerable<Vessel>>(CacheKeys.AllVessels);

                        if (vessels == null)
                        {
                            vessels = await _vesselRepository.GetAllAsync();
                            _cacheService.Set(CacheKeys.AllVessels, vessels, TimeSpan.FromMinutes(5));
                        }
                    }

                    ViewBag.TotalCount = await _vesselRepository.CountAsync();
                    return View(vessels);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vessels list");
                TempData["Error"] = "An error occurred while loading vessels. Please try again.";
                return View(new List<Vessel>());
            }
        }

        // GET: Vessels/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var vessel = await _vesselRepository.GetVesselWithDetailsAsync(id);
                if (vessel == null)
                {
                    TempData["Error"] = $"Vessel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vessel details for ID: {VesselId}", id);
                TempData["Error"] = "An error occurred while loading vessel details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Vessels/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new VesselViewModel
            {
                CurrentStatus = VesselStatus.Available,
                VesselType = VesselType.Aframax
            };
            return View(viewModel);
        }

        // POST: Vessels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VesselViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Check IMO number uniqueness
                if (!await _vesselRepository.IsImoNumberUniqueAsync(viewModel.ImoNumber))
                {
                    ModelState.AddModelError(nameof(viewModel.ImoNumber),
                        "A vessel with this IMO number already exists.");
                    return View(viewModel);
                }

                var vessel = new Vessel
                {
                    VesselName = viewModel.VesselName,
                    ImoNumber = viewModel.ImoNumber,
                    Dwt = viewModel.Dwt,
                    VesselType = viewModel.VesselType,
                    CurrentStatus = viewModel.CurrentStatus
                };

                await _vesselRepository.AddAsync(vessel);
                await _vesselRepository.SaveChangesAsync();

                // Invalidate vessel caches when data changes
                _cacheService.Remove(CacheKeys.AllVessels);
                _cacheService.Remove(CacheKeys.VesselsByStatus(vessel.CurrentStatus.ToString()));
                _cacheService.Remove(CacheKeys.DashboardStats);

                _logger.LogInformation("Created new vessel: {VesselName} ({ImoNumber})",
                    vessel.VesselName, vessel.ImoNumber);

                TempData["Success"] = $"Vessel '{vessel.VesselName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vessel: {VesselName}", viewModel.VesselName);
                ModelState.AddModelError(string.Empty,
                    "An error occurred while creating the vessel. Please try again.");
                return View(viewModel);
            }
        }

        // GET: Vessels/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vessel = await _vesselRepository.GetByIdAsync(id);
                if (vessel == null)
                {
                    TempData["Error"] = $"Vessel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new VesselViewModel
                {
                    Id = vessel.Id,
                    VesselName = vessel.VesselName,
                    ImoNumber = vessel.ImoNumber,
                    Dwt = vessel.Dwt,
                    VesselType = vessel.VesselType,
                    CurrentStatus = vessel.CurrentStatus
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vessel for edit, ID: {VesselId}", id);
                TempData["Error"] = "An error occurred while loading the vessel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vessels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VesselViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "Invalid request.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Check IMO number uniqueness (excluding current vessel)
                if (!await _vesselRepository.IsImoNumberUniqueAsync(viewModel.ImoNumber, viewModel.Id))
                {
                    ModelState.AddModelError(nameof(viewModel.ImoNumber),
                        "A vessel with this IMO number already exists.");
                    return View(viewModel);
                }

                var vessel = await _vesselRepository.GetByIdAsync(id);
                if (vessel == null)
                {
                    TempData["Error"] = $"Vessel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                vessel.VesselName = viewModel.VesselName;
                vessel.ImoNumber = viewModel.ImoNumber;
                vessel.Dwt = viewModel.Dwt;
                vessel.VesselType = viewModel.VesselType;
                vessel.CurrentStatus = viewModel.CurrentStatus;

                await _vesselRepository.UpdateAsync(vessel);
                await _vesselRepository.SaveChangesAsync();

                _logger.LogInformation("Updated vessel: {VesselName} (ID: {VesselId})",
                    vessel.VesselName, vessel.Id);

                TempData["Success"] = $"Vessel '{vessel.VesselName}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = vessel.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vessel ID: {VesselId}", id);
                ModelState.AddModelError(string.Empty,
                    "An error occurred while updating the vessel. Please try again.");
                return View(viewModel);
            }
        }

        // GET: Vessels/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var vessel = await _vesselRepository.GetByIdAsync(id);
                if (vessel == null)
                {
                    TempData["Error"] = $"Vessel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(vessel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vessel for delete, ID: {VesselId}", id);
                TempData["Error"] = "An error occurred while loading the vessel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vessels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var vessel = await _vesselRepository.GetByIdAsync(id);
                if (vessel == null)
                {
                    TempData["Error"] = $"Vessel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                var vesselName = vessel.VesselName;

                await _vesselRepository.DeleteAsync(vessel);
                await _vesselRepository.SaveChangesAsync();

                _logger.LogInformation("Deleted vessel: {VesselName} (ID: {VesselId})",
                    vesselName, id);

                TempData["Success"] = $"Vessel '{vesselName}' deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vessel ID: {VesselId}", id);
                TempData["Error"] = "An error occurred while deleting the vessel. It may be in use by voyage allocations.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
