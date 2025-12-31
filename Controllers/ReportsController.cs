using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.ViewModels;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Controllers;

/// <summary>
/// Advanced reports controller showcasing LINQ's powerful query capabilities
/// Demonstrates complex aggregations, joins, grouping, and projections
/// </summary>
[Authorize(Roles = "Trader,Admin")]
public class ReportsController : Controller
{
    private readonly ICargoParcelRepository _parcelRepository;
    private readonly IVesselRepository _vesselRepository;
    private readonly IVoyageAllocationRepository _allocationRepository;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        ICargoParcelRepository parcelRepository,
        IVesselRepository vesselRepository,
        IVoyageAllocationRepository allocationRepository,
        ILogger<ReportsController> logger)
    {
        _parcelRepository = parcelRepository;
        _vesselRepository = vesselRepository;
        _allocationRepository = allocationRepository;
        _logger = logger;
    }

    // GET: Reports
    public async Task<IActionResult> Index()
    {
        var dashboard = new DashboardReportViewModel
        {
            TotalParcels = await _parcelRepository.CountAsync(),
            TotalVessels = await _vesselRepository.CountAsync(),
            TotalAllocations = await _allocationRepository.CountAsync()
        };

        return View(dashboard);
    }

    // GET: Reports/ParcelSummary
    public async Task<IActionResult> ParcelSummary()
    {
        try
        {
            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .ToListAsync();

            var totalParcels = parcels.Count;

            // Advanced LINQ: GroupBy with complex aggregations
            var summaries = parcels
                .GroupBy(p => p.Status)
                .Select(g => new ParcelSummaryViewModel
                {
                    Status = g.Key.ToString(),
                    ParcelCount = g.Count(),
                    TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                    AverageVolumeBbls = g.Average(p => p.QuantityBbls),
                    MinVolumeBbls = g.Min(p => p.QuantityBbls),
                    MaxVolumeBbls = g.Max(p => p.QuantityBbls),
                    AllocationCount = g.Sum(p => p.VoyageAllocations?.Count ?? 0),
                    PercentageOfTotal = totalParcels > 0 ? (decimal)g.Count() / totalParcels * 100 : 0
                })
                .OrderByDescending(s => s.TotalVolumeBbls)
                .ToList();

            return View(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating parcel summary report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/VesselUtilization
    public async Task<IActionResult> VesselUtilization()
    {
        try
        {
            var vessels = await _vesselRepository.GetQueryable()
                .Include(v => v.VoyageAllocations)
                    .ThenInclude(va => va.CargoParcel)
                .ToListAsync();

            // Advanced LINQ: Complex calculations with nested aggregations
            var utilization = vessels
                .Select(v =>
                {
                    var allocations = v.VoyageAllocations ?? new List<VoyageAllocation>();
                    var daysAtSea = allocations.Sum(va => (va.DischargeDate - va.LoadingDate).Days);
                    var totalDays = 365; // Assuming yearly calculation
                    var daysIdle = Math.Max(0, totalDays - daysAtSea);

                    return new VesselUtilizationViewModel
                    {
                        VesselId = v.Id,
                        VesselName = v.VesselName,
                        ImoNumber = v.ImoNumber,
                        VesselType = v.VesselType.ToString(),
                        CurrentStatus = v.CurrentStatus.ToString(),
                        TotalVoyages = allocations.Count,
                        DaysAtSea = daysAtSea,
                        DaysIdle = daysIdle,
                        UtilizationPercentage = totalDays > 0 ? (decimal)daysAtSea / totalDays * 100 : 0,
                        TotalCargoCarriedBbls = allocations.Sum(va => va.CargoParcel?.QuantityBbls ?? 0),
                        AverageFreightRate = allocations.Any() ? allocations.Average(va => va.FreightRate) : 0,
                        TotalRevenue = allocations.Sum(va => va.FreightRate * (va.CargoParcel?.QuantityBbls ?? 0))
                    };
                })
                .OrderByDescending(v => v.UtilizationPercentage)
                .ThenByDescending(v => v.TotalRevenue)
                .ToList();

            return View(utilization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating vessel utilization report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/CrudeGradeAnalysis
    public async Task<IActionResult> CrudeGradeAnalysis()
    {
        try
        {
            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .ToListAsync();

            var totalVolume = parcels.Sum(p => p.QuantityBbls);

            // Advanced LINQ: Multi-level grouping and complex projections
            var analysis = parcels
                .GroupBy(p => p.CrudeGrade)
                .Select(g =>
                {
                    var gradeAllocations = g
                        .SelectMany(p => p.VoyageAllocations ?? Enumerable.Empty<VoyageAllocation>())
                        .ToList();

                    var portFrequency = g
                        .GroupBy(p => p.LoadingPort)
                        .OrderByDescending(pg => pg.Count())
                        .FirstOrDefault();

                    var statusFrequency = g
                        .GroupBy(p => p.Status)
                        .OrderByDescending(sg => sg.Count())
                        .FirstOrDefault();

                    return new CrudeGradeAnalysisViewModel
                    {
                        CrudeGrade = g.Key,
                        ParcelCount = g.Count(),
                        TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                        AverageVolumeBbls = g.Average(p => p.QuantityBbls),
                        MarketSharePercentage = totalVolume > 0 ? g.Sum(p => p.QuantityBbls) / totalVolume * 100 : 0,
                        AllocationCount = gradeAllocations.Count,
                        AverageFreightRate = gradeAllocations.Any() ? gradeAllocations.Average(a => a.FreightRate) : 0,
                        MinFreightRate = gradeAllocations.Any() ? gradeAllocations.Min(a => a.FreightRate) : 0,
                        MaxFreightRate = gradeAllocations.Any() ? gradeAllocations.Max(a => a.FreightRate) : 0,
                        MostUsedPort = portFrequency?.Key ?? "N/A",
                        MostCommonStatus = statusFrequency?.Key.ToString() ?? "N/A"
                    };
                })
                .OrderByDescending(a => a.TotalVolumeBbls)
                .ToList();

            return View(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating crude grade analysis report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/LaycanCalendar
    public async Task<IActionResult> LaycanCalendar(int daysAhead = 30)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(daysAhead);

            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                    .ThenInclude(va => va.Vessel)
                .Where(p => p.LaycanStart <= endDate && p.LaycanEnd >= today)
                .ToListAsync();

            // Advanced LINQ: Date calculations and conditional logic
            var calendar = parcels
                .Select(p =>
                {
                    var allocation = p.VoyageAllocations?.FirstOrDefault();
                    var daysUntil = (p.LaycanStart - today).Days;
                    var duration = (p.LaycanEnd - p.LaycanStart).Days;

                    string urgency;
                    if (daysUntil < 0)
                        urgency = "Critical";
                    else if (daysUntil <= 7)
                        urgency = "Urgent";
                    else if (daysUntil <= 14)
                        urgency = "Soon";
                    else
                        urgency = "Normal";

                    return new LaycanCalendarViewModel
                    {
                        ParcelId = p.Id,
                        ParcelName = p.ParcelName,
                        CrudeGrade = p.CrudeGrade,
                        QuantityBbls = p.QuantityBbls,
                        LoadingPort = p.LoadingPort,
                        DischargePort = p.DischargePort,
                        LaycanStart = p.LaycanStart,
                        LaycanEnd = p.LaycanEnd,
                        LaycanDuration = duration,
                        DaysUntilLaycan = daysUntil,
                        Status = p.Status.ToString(),
                        IsAllocated = allocation != null,
                        VesselName = allocation?.Vessel?.VesselName,
                        UrgencyLevel = urgency
                    };
                })
                .OrderBy(l => l.LaycanStart)
                .ThenBy(l => l.LoadingPort)
                .ToList();

            ViewBag.DaysAhead = daysAhead;
            return View(calendar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating laycan calendar report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/PortActivity
    public async Task<IActionResult> PortActivity()
    {
        try
        {
            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .ToListAsync();

            // Advanced LINQ: Union of different groupings
            var loadingPorts = parcels
                .GroupBy(p => p.LoadingPort)
                .Select(g => new
                {
                    Port = g.Key,
                    Type = "Loading",
                    Count = g.Count(),
                    Volume = g.Sum(p => p.QuantityBbls),
                    Grades = g.Select(p => p.CrudeGrade).Distinct().ToList()
                });

            var dischargePorts = parcels
                .GroupBy(p => p.DischargePort)
                .Select(g => new
                {
                    Port = g.Key,
                    Type = "Discharge",
                    Count = g.Count(),
                    Volume = g.Sum(p => p.QuantityBbls),
                    Grades = g.Select(p => p.CrudeGrade).Distinct().ToList()
                });

            var portActivity = parcels
                .SelectMany(p => new[] { p.LoadingPort, p.DischargePort })
                .Distinct()
                .Select(port =>
                {
                    var loading = loadingPorts.FirstOrDefault(lp => lp.Port == port);
                    var discharge = dischargePorts.FirstOrDefault(dp => dp.Port == port);

                    var allGrades = new List<string>();
                    if (loading != null) allGrades.AddRange(loading.Grades);
                    if (discharge != null) allGrades.AddRange(discharge.Grades);

                    var allocationsAtPort = parcels
                        .Where(p => p.LoadingPort == port || p.DischargePort == port)
                        .SelectMany(p => p.VoyageAllocations ?? Enumerable.Empty<VoyageAllocation>())
                        .ToList();

                    var avgTurnaround = allocationsAtPort.Any()
                        ? (decimal)allocationsAtPort.Average(a => (a.DischargeDate - a.LoadingDate).Days)
                        : 0;

                    return new PortActivityViewModel
                    {
                        PortName = port,
                        LoadingCount = loading?.Count ?? 0,
                        DischargeCount = discharge?.Count ?? 0,
                        TotalOperations = (loading?.Count ?? 0) + (discharge?.Count ?? 0),
                        TotalVolumeLoaded = loading?.Volume ?? 0,
                        TotalVolumeDischarged = discharge?.Volume ?? 0,
                        TopCrudeGrades = allGrades.Distinct().Take(3).ToList(),
                        AverageTurnaroundDays = avgTurnaround
                    };
                })
                .OrderByDescending(p => p.TotalOperations)
                .ToList();

            return View(portActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating port activity report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/RouteAnalysis
    public async Task<IActionResult> RouteAnalysis()
    {
        try
        {
            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .ToListAsync();

            // Advanced LINQ: Grouping by multiple fields
            var routes = parcels
                .GroupBy(p => new { p.LoadingPort, p.DischargePort })
                .Select(g =>
                {
                    var allocations = g
                        .SelectMany(p => p.VoyageAllocations ?? Enumerable.Empty<VoyageAllocation>())
                        .ToList();

                    return new RouteAnalysisViewModel
                    {
                        LoadingPort = g.Key.LoadingPort,
                        DischargePort = g.Key.DischargePort,
                        Route = $"{g.Key.LoadingPort} → {g.Key.DischargePort}",
                        ShipmentCount = g.Count(),
                        TotalVolume = g.Sum(p => p.QuantityBbls),
                        AverageVolume = g.Average(p => p.QuantityBbls),
                        AverageFreightRate = allocations.Any() ? allocations.Average(a => a.FreightRate) : 0,
                        CrudeGrades = g.Select(p => p.CrudeGrade).Distinct().ToList(),
                        AverageTransitDays = allocations.Any()
                            ? (int)allocations.Average(a => (a.DischargeDate - a.LoadingDate).Days)
                            : 0
                    };
                })
                .OrderByDescending(r => r.ShipmentCount)
                .ThenByDescending(r => r.TotalVolume)
                .ToList();

            return View(routes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating route analysis report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Reports/Financial
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Financial()
    {
        try
        {
            var allocations = await _allocationRepository.GetQueryable()
                .Include(a => a.CargoParcel)
                .Include(a => a.Vessel)
                .ToListAsync();

            // Advanced LINQ: Complex financial calculations
            var totalFreight = allocations.Sum(a => a.FreightRate * (a.CargoParcel?.QuantityBbls ?? 0));
            var totalDemurrage = allocations.Sum(a => a.DemurrageRate * (a.DischargeDate - a.LoadingDate).Days);

            var revenueByType = allocations
                .GroupBy(a => a.Vessel?.VesselType.ToString() ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.FreightRate * (a.CargoParcel?.QuantityBbls ?? 0))
                );

            var revenueByGrade = allocations
                .GroupBy(a => a.CargoParcel?.CrudeGrade ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.FreightRate * (a.CargoParcel?.QuantityBbls ?? 0))
                );

            var summary = new FinancialSummaryViewModel
            {
                TotalFreightRevenue = totalFreight,
                TotalDemurrage = totalDemurrage,
                AverageFreightRate = allocations.Any() ? allocations.Average(a => a.FreightRate) : 0,
                AverageDemurrageRate = allocations.Any() ? allocations.Average(a => a.DemurrageRate) : 0,
                TotalAllocations = allocations.Count,
                ActiveVoyages = allocations.Count(a => a.LoadingDate <= DateTime.UtcNow && a.DischargeDate >= DateTime.UtcNow),
                TotalVolumeTransported = allocations.Sum(a => a.CargoParcel?.QuantityBbls ?? 0),
                RevenueByVesselType = revenueByType,
                RevenueByGrade = revenueByGrade
            };

            return View(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report");
            TempData["ErrorMessage"] = "An error occurred while generating the report";
            return RedirectToAction(nameof(Index));
        }
    }
}
