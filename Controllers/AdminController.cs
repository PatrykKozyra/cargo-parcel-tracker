using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CargoParcelTracker.Services;
using CargoParcelTracker.ViewModels;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Admin-only controller for system monitoring and cache statistics
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly IPerformanceMonitoringService _performanceService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ICacheService cacheService,
            IPerformanceMonitoringService performanceService,
            ILogger<AdminController> logger)
        {
            _cacheService = cacheService;
            _performanceService = performanceService;
            _logger = logger;
        }

        // GET: Admin/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            var cacheStats = _cacheService.GetStatistics();
            var performanceStats = _performanceService.GetStatistics();

            var viewModel = new AdminDashboardViewModel
            {
                CacheStatistics = cacheStats,
                PerformanceStatistics = performanceStats
            };

            return View(viewModel);
        }

        // POST: Admin/ClearCache
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCache()
        {
            // Clear all common caches
            _cacheService.Remove(CacheKeys.AllVessels);
            _cacheService.Remove(CacheKeys.AllParcels);
            _cacheService.Remove(CacheKeys.AllVoyageAllocations);
            _cacheService.Remove(CacheKeys.DashboardStats);

            _logger.LogInformation("Admin cleared all caches");

            TempData["Success"] = "All caches cleared successfully!";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
