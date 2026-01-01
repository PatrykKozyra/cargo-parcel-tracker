# Background Services Documentation

## Overview

This application demonstrates .NET's built-in background processing capabilities using the `IHostedService` interface. Unlike other frameworks that require third-party tools (like Celery in Django), .NET provides native support for background tasks.

## ParcelExpirationService

### Purpose

Automatically monitors and updates cargo parcels that have passed their laycan end date. This service ensures data integrity by marking expired nominated parcels as "Cancelled" without manual intervention.

### Implementation Details

**File**: `Services/ParcelExpirationService.cs`

The service implements two key interfaces:
- `IHostedService`: Provides lifecycle management (start/stop)
- `IDisposable`: Ensures proper cleanup of resources

### Key Features

1. **Automatic Startup**: Service starts automatically when the application launches
2. **Scheduled Execution**: Runs every 5 minutes using a Timer
3. **Scoped Services in Singleton**: Demonstrates proper DbContext usage in background services
4. **Comprehensive Logging**: All activities are logged for monitoring and debugging

### How It Works

```csharp
// Service lifecycle
StartAsync()  → Initializes timer and runs immediately
   ↓
DoWork()     → Executes every 5 minutes
   ↓
   1. Creates a scope for DbContext
   2. Queries for expired parcels (Nominated status + LaycanEnd < Now)
   3. Updates status to Cancelled
   4. Saves changes
   5. Logs all activities
   ↓
StopAsync()  → Cleanup when application shuts down
```

### Service Registration

In `Program.cs`:

```csharp
builder.Services.AddHostedService<ParcelExpirationService>();
```

This single line:
- Registers the service with dependency injection
- Ensures automatic startup/shutdown
- Manages the service lifecycle

### Database Query

The service uses LINQ to find expired parcels:

```csharp
var expiredParcels = dbContext.CargoParcels
    .Where(p => p.Status == CargoParcelStatus.Nominated
             && p.LaycanEnd < DateTime.UtcNow)
    .ToList();
```

### Logging Output

When the service runs, you'll see console output like:

```
info: ParcelExpirationService[0]
      Parcel Expiration Service is starting

info: ParcelExpirationService[0]
      Parcel Expiration Service is working: 01/01/2026 21:22:19 +01:00

warn: ParcelExpirationService[0]
      Found 3 expired parcels. Updating to Cancelled status

info: ParcelExpirationService[0]
      Expiring parcel: ID=15, Name=PARCEL-2025-0015, LaycanEnd=12/09/2025

info: ParcelExpirationService[0]
      Successfully updated 3 expired parcels to Cancelled status
```

## Important Pattern: Scoped Services in Singleton

Background services registered with `AddHostedService` are **singletons** (one instance for the entire application lifetime). However, `DbContext` is **scoped** (one instance per request).

### The Problem

You cannot inject a scoped service (DbContext) into a singleton service directly.

### The Solution

Create a scope manually:

```csharp
using (var scope = _serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Use dbContext here
    // Scope is disposed automatically when exiting the using block
}
```

This pattern:
- Creates a new scope for each background task execution
- Gets a fresh DbContext instance
- Automatically disposes the scope and DbContext when done
- Prevents memory leaks and connection issues

## Advantages Over Other Frameworks

### vs. Django/Celery

**Django + Celery**:
- Requires separate message broker (Redis/RabbitMQ)
- Additional infrastructure to manage
- Separate worker processes
- More complex configuration

**.NET IHostedService**:
- Built into the framework
- No external dependencies
- Runs in the same process
- Simple one-line registration

### vs. Node.js/cron

**Node.js + cron**:
- Third-party libraries (node-cron, agenda)
- Manual lifecycle management
- May require separate processes

**.NET IHostedService**:
- Native framework support
- Automatic lifecycle management
- Integrated with DI container
- Built-in logging integration

## Customization

### Changing the Schedule

Modify the timer period in `StartAsync()`:

```csharp
// Every 1 minute
_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

// Every 1 hour
_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));

// Daily at midnight (more complex, use NCronTab or Quartz.NET)
```

### Adding More Background Tasks

Create additional services implementing `IHostedService`:

```csharp
public class VesselStatusUpdateService : IHostedService
{
    // Implementation
}

// Register in Program.cs
builder.Services.AddHostedService<VesselStatusUpdateService>();
```

### Using BackgroundService

For simpler cases, inherit from `BackgroundService` instead of implementing `IHostedService`:

```csharp
public class MyBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Do work
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Advanced Scenarios

For more complex scheduling requirements, consider:

1. **Hangfire**: Dashboard UI, job persistence, cron expressions
2. **Quartz.NET**: Enterprise-grade job scheduling, cron support
3. **Azure Functions**: Serverless alternative for cloud deployments

However, for most scenarios, `IHostedService` is sufficient and keeps the application simple.

## Best Practices

1. **Always use scopes** when accessing scoped services from background services
2. **Log activities** for monitoring and debugging
3. **Handle exceptions** gracefully to prevent service crashes
4. **Use cancellation tokens** for graceful shutdown
5. **Dispose resources** properly (implement IDisposable)
6. **Consider timing** - avoid long-running operations that block the timer
7. **Test thoroughly** - background services can be tricky to debug

## Testing

To verify the service is working:

1. Run the application: `dotnet run`
2. Check console logs for "Parcel Expiration Service is starting"
3. Wait 5 minutes or check immediately after startup
4. Look for log messages indicating parcels were processed
5. Query the database to confirm status updates

## Monitoring in Production

In production, ensure:
- Application logs are captured (Application Insights, Seq, etc.)
- Set up alerts for service failures
- Monitor execution time and resource usage
- Consider health checks for background services

## Summary

The `ParcelExpirationService` demonstrates:
- ✅ Native .NET background processing
- ✅ No external dependencies required
- ✅ Proper scoped service handling
- ✅ Comprehensive logging
- ✅ Automatic lifecycle management
- ✅ Simple, maintainable code

This pattern is production-ready and scales well for most business applications.
