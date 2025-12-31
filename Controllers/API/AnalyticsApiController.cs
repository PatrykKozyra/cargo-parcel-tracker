using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.DTOs;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Controllers.API;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
public class AnalyticsApiController : ControllerBase
{
    private readonly ICargoParcelRepository _parcelRepository;
    private readonly IVesselRepository _vesselRepository;
    private readonly IVoyageAllocationRepository _allocationRepository;
    private readonly ILogger<AnalyticsApiController> _logger;

    public AnalyticsApiController(
        ICargoParcelRepository parcelRepository,
        IVesselRepository vesselRepository,
        IVoyageAllocationRepository allocationRepository,
        ILogger<AnalyticsApiController> logger)
    {
        _parcelRepository = parcelRepository;
        _vesselRepository = vesselRepository;
        _allocationRepository = allocationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get total volume by crude grade
    /// </summary>
    [HttpGet("volume-by-grade")]
    [ProducesResponseType(typeof(IEnumerable<VolumeByGradeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VolumeByGradeDto>>> GetVolumeByGrade()
    {
        try
        {
            var parcels = await _parcelRepository.GetAllAsync();

            var volumeByGrade = parcels
                .GroupBy(p => p.CrudeGrade)
                .Select(g => new VolumeByGradeDto
                {
                    CrudeGrade = g.Key,
                    TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                    ParcelCount = g.Count(),
                    AverageVolumeBbls = g.Average(p => p.QuantityBbls)
                })
                .OrderByDescending(v => v.TotalVolumeBbls)
                .ToList();

            return Ok(volumeByGrade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating volume by grade");
            return StatusCode(500, "An error occurred while calculating volume by grade");
        }
    }

    /// <summary>
    /// Get parcel count and volume by status
    /// </summary>
    [HttpGet("parcels-by-status")]
    [ProducesResponseType(typeof(IEnumerable<ParcelsByStatusDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParcelsByStatusDto>>> GetParcelsByStatus()
    {
        try
        {
            var parcels = await _parcelRepository.GetAllAsync();
            var totalParcels = parcels.Count();

            var parcelsByStatus = parcels
                .GroupBy(p => p.Status)
                .Select(g => new ParcelsByStatusDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                    Percentage = totalParcels > 0 ? (decimal)g.Count() / totalParcels * 100 : 0
                })
                .OrderByDescending(p => p.Count)
                .ToList();

            return Ok(parcelsByStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating parcels by status");
            return StatusCode(500, "An error occurred while calculating parcels by status");
        }
    }

    /// <summary>
    /// Get vessel utilization statistics by vessel type
    /// </summary>
    [HttpGet("vessel-utilization")]
    [ProducesResponseType(typeof(IEnumerable<VesselUtilizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VesselUtilizationDto>>> GetVesselUtilization()
    {
        try
        {
            var vessels = await _vesselRepository.GetAllAsync();

            var utilization = vessels
                .GroupBy(v => v.VesselType)
                .Select(g => new VesselUtilizationDto
                {
                    VesselType = g.Key.ToString(),
                    TotalVessels = g.Count(),
                    AvailableVessels = g.Count(v => v.CurrentStatus == VesselStatus.Available),
                    InUseVessels = g.Count(v => v.CurrentStatus != VesselStatus.Available),
                    UtilizationPercentage = g.Count() > 0
                        ? (decimal)g.Count(v => v.CurrentStatus != VesselStatus.Available) / g.Count() * 100
                        : 0
                })
                .OrderBy(u => u.VesselType)
                .ToList();

            return Ok(utilization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating vessel utilization");
            return StatusCode(500, "An error occurred while calculating vessel utilization");
        }
    }

    /// <summary>
    /// Get comprehensive dashboard summary with key metrics
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        try
        {
            var vessels = await _vesselRepository.GetAllAsync();
            var parcels = await _parcelRepository.GetAllAsync();
            var allocations = await _allocationRepository.GetAllAsync();

            var totalParcels = parcels.Count();

            var volumeByGrade = parcels
                .GroupBy(p => p.CrudeGrade)
                .Select(g => new VolumeByGradeDto
                {
                    CrudeGrade = g.Key,
                    TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                    ParcelCount = g.Count(),
                    AverageVolumeBbls = g.Average(p => p.QuantityBbls)
                })
                .OrderByDescending(v => v.TotalVolumeBbls)
                .ToList();

            var parcelsByStatus = parcels
                .GroupBy(p => p.Status)
                .Select(g => new ParcelsByStatusDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalVolumeBbls = g.Sum(p => p.QuantityBbls),
                    Percentage = totalParcels > 0 ? (decimal)g.Count() / totalParcels * 100 : 0
                })
                .OrderByDescending(p => p.Count)
                .ToList();

            var summary = new DashboardSummaryDto
            {
                TotalVessels = vessels.Count(),
                TotalParcels = totalParcels,
                TotalAllocations = allocations.Count(),
                AvailableVessels = vessels.Count(v => v.CurrentStatus == VesselStatus.Available),
                TotalVolumeBbls = parcels.Sum(p => p.QuantityBbls),
                VolumeByGrade = volumeByGrade,
                ParcelsByStatus = parcelsByStatus
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard summary");
            return StatusCode(500, "An error occurred while generating the dashboard summary");
        }
    }

    /// <summary>
    /// Get top parcels by volume
    /// </summary>
    [HttpGet("top-parcels")]
    [ProducesResponseType(typeof(IEnumerable<CargoParcelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CargoParcelDto>>> GetTopParcelsByVolume([FromQuery] int limit = 10)
    {
        try
        {
            limit = limit < 1 ? 10 : (limit > 50 ? 50 : limit);

            var parcels = await _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .OrderByDescending(p => p.QuantityBbls)
                .Take(limit)
                .ToListAsync();

            var parcelDtos = parcels.Select(p => new CargoParcelDto
            {
                Id = p.Id,
                ParcelName = p.ParcelName,
                CrudeGrade = p.CrudeGrade,
                QuantityBbls = p.QuantityBbls,
                LoadingPort = p.LoadingPort,
                DischargePort = p.DischargePort,
                LaycanStart = p.LaycanStart,
                LaycanEnd = p.LaycanEnd,
                Status = p.Status.ToString(),
                CreatedDate = p.CreatedDate,
                VoyageAllocationCount = p.VoyageAllocations?.Count ?? 0
            }).ToList();

            return Ok(parcelDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top parcels by volume");
            return StatusCode(500, "An error occurred while retrieving top parcels");
        }
    }

    /// <summary>
    /// Get statistics for a specific crude grade
    /// </summary>
    [HttpGet("grade/{grade}")]
    [ProducesResponseType(typeof(VolumeByGradeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VolumeByGradeDto>> GetGradeStatistics(string grade)
    {
        try
        {
            var parcels = await _parcelRepository.GetAllAsync();
            var gradeParcels = parcels.Where(p => p.CrudeGrade.Equals(grade, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!gradeParcels.Any())
            {
                return NotFound(new { message = $"No parcels found for crude grade '{grade}'" });
            }

            var stats = new VolumeByGradeDto
            {
                CrudeGrade = grade,
                TotalVolumeBbls = gradeParcels.Sum(p => p.QuantityBbls),
                ParcelCount = gradeParcels.Count,
                AverageVolumeBbls = gradeParcels.Average(p => p.QuantityBbls)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for grade {Grade}", grade);
            return StatusCode(500, "An error occurred while retrieving grade statistics");
        }
    }
}
