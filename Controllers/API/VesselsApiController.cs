using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.DTOs;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Controllers.API;

[ApiController]
[Route("api/vessels")]
[Produces("application/json")]
public class VesselsApiController : ControllerBase
{
    private readonly IVesselRepository _vesselRepository;
    private readonly ILogger<VesselsApiController> _logger;

    public VesselsApiController(
        IVesselRepository vesselRepository,
        ILogger<VesselsApiController> logger)
    {
        _vesselRepository = vesselRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all vessels with pagination, filtering, and sorting
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="vesselType">Filter by vessel type</param>
    /// <param name="status">Filter by status</param>
    /// <param name="sortBy">Sort by field (VesselName, Dwt, VesselType)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VesselDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<VesselDto>>> GetAllVessels(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? vesselType = null,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "VesselName",
        [FromQuery] string sortOrder = "asc")
    {
        try
        {
            // Validate pagination parameters
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            // Get queryable with includes
            var query = _vesselRepository.GetQueryable()
                .Include(v => v.VoyageAllocations)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(vesselType))
            {
                query = query.Where(v => v.VesselType.ToString() == vesselType);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(v => v.CurrentStatus.ToString() == status);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "vesselname" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(v => v.VesselName)
                    : query.OrderBy(v => v.VesselName),
                "dwt" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(v => v.Dwt)
                    : query.OrderBy(v => v.Dwt),
                "vesseltype" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(v => v.VesselType)
                    : query.OrderBy(v => v.VesselType),
                _ => query.OrderBy(v => v.VesselName)
            };

            // Apply pagination
            var vessels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
            var vesselDtos = vessels.Select(v => new VesselDto
            {
                Id = v.Id,
                VesselName = v.VesselName,
                ImoNumber = v.ImoNumber,
                Dwt = v.Dwt,
                VesselType = v.VesselType.ToString(),
                CurrentStatus = v.CurrentStatus.ToString(),
                VoyageAllocationCount = v.VoyageAllocations?.Count ?? 0
            }).ToList();

            var result = new PagedResultDto<VesselDto>
            {
                Items = vesselDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vessels");
            return StatusCode(500, "An error occurred while retrieving vessels");
        }
    }

    /// <summary>
    /// Get a specific vessel by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VesselDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VesselDto>> GetVesselById(int id)
    {
        try
        {
            var vessel = await _vesselRepository.GetVesselWithDetailsAsync(id);

            if (vessel == null)
            {
                return NotFound(new { message = $"Vessel with ID {id} not found" });
            }

            var dto = new VesselDto
            {
                Id = vessel.Id,
                VesselName = vessel.VesselName,
                ImoNumber = vessel.ImoNumber,
                Dwt = vessel.Dwt,
                VesselType = vessel.VesselType.ToString(),
                CurrentStatus = vessel.CurrentStatus.ToString(),
                VoyageAllocationCount = vessel.VoyageAllocations?.Count ?? 0
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vessel {VesselId}", id);
            return StatusCode(500, "An error occurred while retrieving the vessel");
        }
    }

    /// <summary>
    /// Create a new vessel
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(VesselDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VesselDto>> CreateVessel([FromBody] VesselCreateDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if IMO number already exists
            var existingVessel = await _vesselRepository.GetVesselByImoNumberAsync(createDto.ImoNumber);
            if (existingVessel != null)
            {
                return BadRequest(new { message = $"Vessel with IMO number {createDto.ImoNumber} already exists" });
            }

            // Parse enums
            if (!Enum.TryParse<VesselType>(createDto.VesselType, out var vesselType))
            {
                return BadRequest(new { message = "Invalid vessel type value" });
            }

            if (!Enum.TryParse<VesselStatus>(createDto.CurrentStatus, out var status))
            {
                return BadRequest(new { message = "Invalid status value" });
            }

            var vessel = new Vessel
            {
                VesselName = createDto.VesselName,
                ImoNumber = createDto.ImoNumber,
                Dwt = createDto.Dwt,
                VesselType = vesselType,
                CurrentStatus = status
            };

            var created = await _vesselRepository.AddAsync(vessel);
            await _vesselRepository.SaveChangesAsync();

            var dto = new VesselDto
            {
                Id = created.Id,
                VesselName = created.VesselName,
                ImoNumber = created.ImoNumber,
                Dwt = created.Dwt,
                VesselType = created.VesselType.ToString(),
                CurrentStatus = created.CurrentStatus.ToString(),
                VoyageAllocationCount = 0
            };

            return CreatedAtAction(nameof(GetVesselById), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vessel");
            return StatusCode(500, "An error occurred while creating the vessel");
        }
    }

    /// <summary>
    /// Update an existing vessel
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateVessel(int id, [FromBody] VesselUpdateDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vessel = await _vesselRepository.GetByIdAsync(id);
            if (vessel == null)
            {
                return NotFound(new { message = $"Vessel with ID {id} not found" });
            }

            // Parse enums
            if (!Enum.TryParse<VesselType>(updateDto.VesselType, out var vesselType))
            {
                return BadRequest(new { message = "Invalid vessel type value" });
            }

            if (!Enum.TryParse<VesselStatus>(updateDto.CurrentStatus, out var status))
            {
                return BadRequest(new { message = "Invalid status value" });
            }

            vessel.VesselName = updateDto.VesselName;
            vessel.Dwt = updateDto.Dwt;
            vessel.VesselType = vesselType;
            vessel.CurrentStatus = status;

            await _vesselRepository.UpdateAsync(vessel);
            await _vesselRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vessel {VesselId}", id);
            return StatusCode(500, "An error occurred while updating the vessel");
        }
    }

    /// <summary>
    /// Delete a vessel
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVessel(int id)
    {
        try
        {
            var vessel = await _vesselRepository.GetByIdAsync(id);
            if (vessel == null)
            {
                return NotFound(new { message = $"Vessel with ID {id} not found" });
            }

            await _vesselRepository.DeleteAsync(vessel);
            await _vesselRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vessel {VesselId}", id);
            return StatusCode(500, "An error occurred while deleting the vessel");
        }
    }

    /// <summary>
    /// Get available vessels by status
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<VesselDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VesselDto>>> GetAvailableVessels()
    {
        try
        {
            var vessels = await _vesselRepository.GetAvailableVesselsAsync();

            var vesselDtos = vessels.Select(v => new VesselDto
            {
                Id = v.Id,
                VesselName = v.VesselName,
                ImoNumber = v.ImoNumber,
                Dwt = v.Dwt,
                VesselType = v.VesselType.ToString(),
                CurrentStatus = v.CurrentStatus.ToString(),
                VoyageAllocationCount = 0
            }).ToList();

            return Ok(vesselDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available vessels");
            return StatusCode(500, "An error occurred while retrieving available vessels");
        }
    }
}
