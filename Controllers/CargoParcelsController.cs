using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.ViewModels;
using CargoParcelTracker.Hubs;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Controller for managing CargoParcel entities with full CRUD operations
    /// Demonstrates custom validation, filtering, and TempData messaging
    /// Includes SignalR for real-time updates
    /// Traders can view/create parcels, Admins have full access
    /// </summary>
    [Authorize(Roles = "Trader,Admin")]
    public class CargoParcelsController : Controller
    {
        private readonly ICargoParcelRepository _parcelRepository;
        private readonly ILogger<CargoParcelsController> _logger;
        private readonly IHubContext<ParcelStatusHub> _hubContext;

        public CargoParcelsController(
            ICargoParcelRepository parcelRepository,
            ILogger<CargoParcelsController> logger,
            IHubContext<ParcelStatusHub> hubContext)
        {
            _parcelRepository = parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        // GET: CargoParcels
        [HttpGet]
        public async Task<IActionResult> Index(CargoParcelStatus? status, string? searchGrade)
        {
            try
            {
                IEnumerable<CargoParcel> parcels;

                if (status.HasValue)
                {
                    parcels = await _parcelRepository.GetParcelsByStatusAsync(status.Value);
                    ViewBag.FilterStatus = status.Value.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(searchGrade))
                {
                    parcels = await _parcelRepository.GetParcelsByCrudeGradeAsync(searchGrade);
                    ViewBag.SearchGrade = searchGrade;
                }
                else
                {
                    parcels = await _parcelRepository.GetAllAsync();
                }

                ViewBag.TotalCount = await _parcelRepository.CountAsync();
                return View(parcels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cargo parcels list");
                TempData["Error"] = "An error occurred while loading cargo parcels. Please try again.";
                return View(new List<CargoParcel>());
            }
        }

        // GET: CargoParcels/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var parcel = await _parcelRepository.GetParcelWithDetailsAsync(id);
                if (parcel == null)
                {
                    TempData["Error"] = $"Cargo parcel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(parcel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cargo parcel details for ID: {ParcelId}", id);
                TempData["Error"] = "An error occurred while loading parcel details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: CargoParcels/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CargoParcelViewModel
            {
                Status = CargoParcelStatus.Planned,
                LaycanStart = DateTime.Today,
                LaycanEnd = DateTime.Today.AddDays(7)
            };
            return View(viewModel);
        }

        // POST: CargoParcels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CargoParcelViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var parcel = new CargoParcel
                {
                    ParcelName = viewModel.ParcelName,
                    CrudeGrade = viewModel.CrudeGrade,
                    QuantityBbls = viewModel.QuantityBbls,
                    LoadingPort = viewModel.LoadingPort,
                    DischargePort = viewModel.DischargePort,
                    LaycanStart = viewModel.LaycanStart,
                    LaycanEnd = viewModel.LaycanEnd,
                    Status = viewModel.Status,
                    CreatedDate = DateTime.UtcNow
                };

                await _parcelRepository.AddAsync(parcel);
                await _parcelRepository.SaveChangesAsync();

                _logger.LogInformation("Created new cargo parcel: {ParcelName}", parcel.ParcelName);

                // Broadcast real-time notification via SignalR
                await _hubContext.Clients.Group("ParcelUpdates").SendAsync("NewParcelCreated", new
                {
                    parcelId = parcel.Id,
                    parcelName = parcel.ParcelName,
                    crudeGrade = parcel.CrudeGrade,
                    quantity = parcel.QuantityBbls,
                    userName = User.Identity?.Name ?? "Unknown",
                    timestamp = DateTime.UtcNow
                });

                TempData["Success"] = $"Cargo parcel '{parcel.ParcelName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cargo parcel: {ParcelName}", viewModel.ParcelName);
                ModelState.AddModelError(string.Empty,
                    "An error occurred while creating the cargo parcel. Please try again.");
                return View(viewModel);
            }
        }

        // GET: CargoParcels/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var parcel = await _parcelRepository.GetByIdAsync(id);
                if (parcel == null)
                {
                    TempData["Error"] = $"Cargo parcel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CargoParcelViewModel
                {
                    Id = parcel.Id,
                    ParcelName = parcel.ParcelName,
                    CrudeGrade = parcel.CrudeGrade,
                    QuantityBbls = parcel.QuantityBbls,
                    LoadingPort = parcel.LoadingPort,
                    DischargePort = parcel.DischargePort,
                    LaycanStart = parcel.LaycanStart,
                    LaycanEnd = parcel.LaycanEnd,
                    Status = parcel.Status,
                    CreatedDate = parcel.CreatedDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cargo parcel for edit, ID: {ParcelId}", id);
                TempData["Error"] = "An error occurred while loading the cargo parcel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CargoParcels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CargoParcelViewModel viewModel)
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
                var parcel = await _parcelRepository.GetByIdAsync(id);
                if (parcel == null)
                {
                    TempData["Error"] = $"Cargo parcel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Store old status for real-time notification
                var oldStatus = parcel.Status;

                parcel.ParcelName = viewModel.ParcelName;
                parcel.CrudeGrade = viewModel.CrudeGrade;
                parcel.QuantityBbls = viewModel.QuantityBbls;
                parcel.LoadingPort = viewModel.LoadingPort;
                parcel.DischargePort = viewModel.DischargePort;
                parcel.LaycanStart = viewModel.LaycanStart;
                parcel.LaycanEnd = viewModel.LaycanEnd;
                parcel.Status = viewModel.Status;

                await _parcelRepository.UpdateAsync(parcel);
                await _parcelRepository.SaveChangesAsync();

                _logger.LogInformation("Updated cargo parcel: {ParcelName} (ID: {ParcelId})",
                    parcel.ParcelName, parcel.Id);

                // Broadcast status change if status was updated
                if (oldStatus != parcel.Status)
                {
                    await _hubContext.Clients.Group("ParcelUpdates").SendAsync("ParcelStatusChanged", new
                    {
                        parcelId = parcel.Id,
                        parcelName = parcel.ParcelName,
                        oldStatus = oldStatus.ToString(),
                        newStatus = parcel.Status.ToString(),
                        userName = User.Identity?.Name ?? "Unknown",
                        timestamp = DateTime.UtcNow
                    });
                }

                TempData["Success"] = $"Cargo parcel '{parcel.ParcelName}' updated successfully!";
                return RedirectToAction(nameof(Details), new { id = parcel.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cargo parcel ID: {ParcelId}", id);
                ModelState.AddModelError(string.Empty,
                    "An error occurred while updating the cargo parcel. Please try again.");
                return View(viewModel);
            }
        }

        // GET: CargoParcels/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var parcel = await _parcelRepository.GetByIdAsync(id);
                if (parcel == null)
                {
                    TempData["Error"] = $"Cargo parcel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(parcel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cargo parcel for delete, ID: {ParcelId}", id);
                TempData["Error"] = "An error occurred while loading the cargo parcel.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: CargoParcels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var parcel = await _parcelRepository.GetByIdAsync(id);
                if (parcel == null)
                {
                    TempData["Error"] = $"Cargo parcel with ID {id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                var parcelName = parcel.ParcelName;

                await _parcelRepository.DeleteAsync(parcel);
                await _parcelRepository.SaveChangesAsync();

                _logger.LogInformation("Deleted cargo parcel: {ParcelName} (ID: {ParcelId})",
                    parcelName, id);

                TempData["Success"] = $"Cargo parcel '{parcelName}' deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cargo parcel ID: {ParcelId}", id);
                TempData["Error"] = "An error occurred while deleting the cargo parcel. It may have voyage allocations.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
