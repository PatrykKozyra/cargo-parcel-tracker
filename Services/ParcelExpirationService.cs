using CargoParcelTracker.Data;
using CargoParcelTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace CargoParcelTracker.Services
{
    /// <summary>
    /// Background service that automatically updates expired cargo parcels.
    /// Demonstrates .NET's IHostedService pattern for background processing.
    /// </summary>
    public class ParcelExpirationService : IHostedService, IDisposable
    {
        private readonly ILogger<ParcelExpirationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public ParcelExpirationService(
            ILogger<ParcelExpirationService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Starts the background service when the application starts.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Parcel Expiration Service is starting");

            // Run immediately on startup, then every 5 minutes
            _timer = new Timer(
                callback: DoWork,
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMinutes(5)
            );

            return Task.CompletedTask;
        }

        /// <summary>
        /// The main background task that checks and updates expired parcels.
        /// Uses scoped services correctly in a singleton context.
        /// </summary>
        private void DoWork(object? state)
        {
            _logger.LogInformation("Parcel Expiration Service is working: {Time}", DateTimeOffset.Now);

            // Create a scope to get scoped services (DbContext is scoped)
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Find all parcels that are:
                    // 1. Status is "Nominated"
                    // 2. LaycanEnd date has passed
                    var expiredParcels = dbContext.CargoParcels
                        .Where(p => p.Status == CargoParcelStatus.Nominated
                                 && p.LaycanEnd < DateTime.UtcNow)
                        .ToList();

                    if (expiredParcels.Any())
                    {
                        _logger.LogWarning(
                            "Found {Count} expired parcels. Updating to Cancelled status",
                            expiredParcels.Count
                        );

                        foreach (var parcel in expiredParcels)
                        {
                            _logger.LogInformation(
                                "Expiring parcel: ID={Id}, Name={Name}, LaycanEnd={LaycanEnd}",
                                parcel.Id,
                                parcel.ParcelName,
                                parcel.LaycanEnd
                            );

                            // Update status to Cancelled (expired)
                            parcel.Status = CargoParcelStatus.Cancelled;
                        }

                        // Save all changes
                        dbContext.SaveChanges();

                        _logger.LogInformation(
                            "Successfully updated {Count} expired parcels to Cancelled status",
                            expiredParcels.Count
                        );
                    }
                    else
                    {
                        _logger.LogInformation("No expired parcels found");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while processing expired parcels"
                    );
                }
            }
        }

        /// <summary>
        /// Stops the background service when the application shuts down.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Parcel Expiration Service is stopping");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clean up resources.
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
