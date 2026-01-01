using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Data;
using CargoParcelTracker.Services;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring application status
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cacheService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<HealthController> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// GET /health
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            var health = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                application = "Cargo Parcel Tracker",
                version = "1.0.0"
            };

            return Ok(health);
        }

        /// <summary>
        /// Detailed health check with database and cache status
        /// GET /health/detailed
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> Detailed()
        {
            var checks = new List<HealthCheckResult>();

            // Database check
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                checks.Add(new HealthCheckResult
                {
                    Component = "Database",
                    Status = canConnect ? "Healthy" : "Unhealthy",
                    ResponseTime = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                checks.Add(new HealthCheckResult
                {
                    Component = "Database",
                    Status = "Unhealthy",
                    Error = ex.Message
                });
            }

            // Cache check
            try
            {
                var cacheStats = _cacheService.GetStatistics();
                checks.Add(new HealthCheckResult
                {
                    Component = "Cache",
                    Status = "Healthy",
                    Details = new
                    {
                        hits = cacheStats.Hits,
                        misses = cacheStats.Misses,
                        hitRate = cacheStats.HitRate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache health check failed");
                checks.Add(new HealthCheckResult
                {
                    Component = "Cache",
                    Status = "Degraded",
                    Error = ex.Message
                });
            }

            var overallStatus = checks.All(c => c.Status == "Healthy") ? "Healthy" :
                                checks.Any(c => c.Status == "Unhealthy") ? "Unhealthy" : "Degraded";

            var health = new
            {
                status = overallStatus,
                timestamp = DateTime.UtcNow,
                checks
            };

            var statusCode = overallStatus == "Healthy" ? 200 : overallStatus == "Degraded" ? 200 : 503;
            return StatusCode(statusCode, health);
        }
    }

    public class HealthCheckResult
    {
        public string Component { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ResponseTime { get; set; }
        public string? Error { get; set; }
        public object? Details { get; set; }
    }
}
