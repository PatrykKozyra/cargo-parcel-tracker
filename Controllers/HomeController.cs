using Microsoft.AspNetCore.Mvc;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;
using System.Diagnostics;

namespace CargoParcelTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVesselRepository _vesselRepository;
        private readonly ICargoParcelRepository _parcelRepository;
        private readonly IVoyageAllocationRepository _allocationRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IVesselRepository vesselRepository,
            ICargoParcelRepository parcelRepository,
            IVoyageAllocationRepository allocationRepository)
        {
            _logger = logger;
            _vesselRepository = vesselRepository;
            _parcelRepository = parcelRepository;
            _allocationRepository = allocationRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Demonstrate Repository Pattern usage
            var vesselCount = await _vesselRepository.CountAsync();
            var parcelCount = await _parcelRepository.CountAsync();
            var allocationCount = await _allocationRepository.CountAsync();
            var availableVessels = await _vesselRepository.GetAvailableVesselsAsync();

            ViewBag.VesselCount = vesselCount;
            ViewBag.ParcelCount = parcelCount;
            ViewBag.AllocationCount = allocationCount;
            ViewBag.AvailableVesselCount = availableVessels.Count();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
