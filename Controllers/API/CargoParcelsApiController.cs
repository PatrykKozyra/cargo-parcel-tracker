using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.DTOs;
using CargoParcelTracker.Repositories.Interfaces;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Controllers.API;

[ApiController]
[Route("api/parcels")]
[Produces("application/json")]
public class CargoParcelsApiController : ControllerBase
{
    private readonly ICargoParcelRepository _parcelRepository;
    private readonly ILogger<CargoParcelsApiController> _logger;

    public CargoParcelsApiController(
        ICargoParcelRepository parcelRepository,
        ILogger<CargoParcelsApiController> logger)
    {
        _parcelRepository = parcelRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all cargo parcels with pagination, filtering, and sorting
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="status">Filter by status</param>
    /// <param name="crudeGrade">Filter by crude grade</param>
    /// <param name="sortBy">Sort by field (ParcelName, QuantityBbls, LaycanStart)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<CargoParcelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<CargoParcelDto>>> GetAllParcels(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? crudeGrade = null,
        [FromQuery] string sortBy = "ParcelName",
        [FromQuery] string sortOrder = "asc")
    {
        try
        {
            // Validate pagination parameters
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            // Get queryable with includes
            var query = _parcelRepository.GetQueryable()
                .Include(p => p.VoyageAllocations)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status.ToString() == status);
            }

            if (!string.IsNullOrEmpty(crudeGrade))
            {
                query = query.Where(p => p.CrudeGrade.Contains(crudeGrade));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "parcelname" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.ParcelName)
                    : query.OrderBy(p => p.ParcelName),
                "quantitybbls" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.QuantityBbls)
                    : query.OrderBy(p => p.QuantityBbls),
                "laycanstart" => sortOrder.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.LaycanStart)
                    : query.OrderBy(p => p.LaycanStart),
                _ => query.OrderBy(p => p.ParcelName)
            };

            // Apply pagination
            var parcels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to DTOs
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

            var result = new PagedResultDto<CargoParcelDto>
            {
                Items = parcelDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cargo parcels");
            return StatusCode(500, "An error occurred while retrieving cargo parcels");
        }
    }

    /// <summary>
    /// Get a specific cargo parcel by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CargoParcelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CargoParcelDto>> GetParcelById(int id)
    {
        try
        {
            var parcel = await _parcelRepository.GetParcelWithDetailsAsync(id);

            if (parcel == null)
            {
                return NotFound(new { message = $"Cargo parcel with ID {id} not found" });
            }

            var dto = new CargoParcelDto
            {
                Id = parcel.Id,
                ParcelName = parcel.ParcelName,
                CrudeGrade = parcel.CrudeGrade,
                QuantityBbls = parcel.QuantityBbls,
                LoadingPort = parcel.LoadingPort,
                DischargePort = parcel.DischargePort,
                LaycanStart = parcel.LaycanStart,
                LaycanEnd = parcel.LaycanEnd,
                Status = parcel.Status.ToString(),
                CreatedDate = parcel.CreatedDate,
                VoyageAllocationCount = parcel.VoyageAllocations?.Count ?? 0
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cargo parcel {ParcelId}", id);
            return StatusCode(500, "An error occurred while retrieving the cargo parcel");
        }
    }

    /// <summary>
    /// Create a new cargo parcel
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CargoParcelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CargoParcelDto>> CreateParcel([FromBody] CargoParcelCreateDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Parse status enum
            if (!Enum.TryParse<CargoParcelStatus>(createDto.Status, out var status))
            {
                return BadRequest(new { message = "Invalid status value" });
            }

            var parcel = new CargoParcel
            {
                ParcelName = createDto.ParcelName,
                CrudeGrade = createDto.CrudeGrade,
                QuantityBbls = createDto.QuantityBbls,
                LoadingPort = createDto.LoadingPort,
                DischargePort = createDto.DischargePort,
                LaycanStart = createDto.LaycanStart,
                LaycanEnd = createDto.LaycanEnd,
                Status = status,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _parcelRepository.AddAsync(parcel);
            await _parcelRepository.SaveChangesAsync();

            var dto = new CargoParcelDto
            {
                Id = created.Id,
                ParcelName = created.ParcelName,
                CrudeGrade = created.CrudeGrade,
                QuantityBbls = created.QuantityBbls,
                LoadingPort = created.LoadingPort,
                DischargePort = created.DischargePort,
                LaycanStart = created.LaycanStart,
                LaycanEnd = created.LaycanEnd,
                Status = created.Status.ToString(),
                CreatedDate = created.CreatedDate,
                VoyageAllocationCount = 0
            };

            return CreatedAtAction(nameof(GetParcelById), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cargo parcel");
            return StatusCode(500, "An error occurred while creating the cargo parcel");
        }
    }

    /// <summary>
    /// Update an existing cargo parcel
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParcel(int id, [FromBody] CargoParcelUpdateDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var parcel = await _parcelRepository.GetByIdAsync(id);
            if (parcel == null)
            {
                return NotFound(new { message = $"Cargo parcel with ID {id} not found" });
            }

            // Parse status enum
            if (!Enum.TryParse<CargoParcelStatus>(updateDto.Status, out var status))
            {
                return BadRequest(new { message = "Invalid status value" });
            }

            parcel.ParcelName = updateDto.ParcelName;
            parcel.CrudeGrade = updateDto.CrudeGrade;
            parcel.QuantityBbls = updateDto.QuantityBbls;
            parcel.LoadingPort = updateDto.LoadingPort;
            parcel.DischargePort = updateDto.DischargePort;
            parcel.LaycanStart = updateDto.LaycanStart;
            parcel.LaycanEnd = updateDto.LaycanEnd;
            parcel.Status = status;

            await _parcelRepository.UpdateAsync(parcel);
            await _parcelRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cargo parcel {ParcelId}", id);
            return StatusCode(500, "An error occurred while updating the cargo parcel");
        }
    }

    /// <summary>
    /// Delete a cargo parcel
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteParcel(int id)
    {
        try
        {
            var parcel = await _parcelRepository.GetByIdAsync(id);
            if (parcel == null)
            {
                return NotFound(new { message = $"Cargo parcel with ID {id} not found" });
            }

            await _parcelRepository.DeleteAsync(parcel);
            await _parcelRepository.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cargo parcel {ParcelId}", id);
            return StatusCode(500, "An error occurred while deleting the cargo parcel");
        }
    }
}
