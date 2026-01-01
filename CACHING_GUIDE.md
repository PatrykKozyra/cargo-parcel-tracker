# Caching & Performance Monitoring Guide

## Overview

This application demonstrates .NET's comprehensive built-in caching system without requiring external dependencies like Redis or Memcached (though it supports them in production). The caching layer significantly improves performance by reducing database queries.

## Caching Architecture

### 1. Memory Cache (IMemoryCache)
- **Type**: In-process, in-memory caching
- **Best for**: Single-server deployments
- **Speed**: Extremely fast (nanoseconds)
- **Persistence**: Lost on application restart
- **Scalability**: Single server only

### 2. Distributed Cache (IDistributedCache)
- **Type**: Shared cache across multiple servers
- **Current Setup**: In-memory implementation (development)
- **Production Options**:
  - Redis
  - SQL Server
  - NCache
- **Best for**: Multi-server, load-balanced deployments

### 3. Response Caching
- **Type**: HTTP response caching middleware
- **Best for**: API endpoints
- **Features**: Automatic HTTP cache headers

## Implementation Details

### Cache Service (`Services/CacheService.cs`)

The custom cache service provides:
- Unified interface for both memory and distributed caching
- Automatic cache hit/miss statistics tracking
- Configurable expiration times
- Generic type support

```csharp
// Get from cache or fetch from database
var vessels = _cacheService.Get<IEnumerable<Vessel>>(CacheKeys.AllVessels);

if (vessels == null)
{
    vessels = await _vesselRepository.GetAllAsync();
    _cacheService.Set(CacheKeys.AllVessels, vessels, TimeSpan.FromMinutes(5));
}
```

### Cache Keys (`Services/CacheKeys.cs`)

Centralized cache key management prevents typos and improves maintainability:

```csharp
public static class CacheKeys
{
    public const string AllVessels = "vessels:all";
    public static string VesselById(int id) => $"vessel:{id}";
    public static string VesselsByStatus(string status) => $"vessels:status:{status}";
}
```

### Performance Monitoring (`Services/PerformanceMonitoringService.cs`)

Automatic operation timing and statistics:

```csharp
using (_performanceService.MeasureOperation("Vessels.Index"))
{
    // Your code here
    // Execution time is automatically tracked
}
```

## Cache Strategy

### What Gets Cached?

1. **Vessel List** (5 minutes)
   - All vessels
   - Vessels by type
   - Vessels by status

2. **Dashboard Statistics** (10 minutes)
   - Total counts
   - Aggregated metrics

3. **API Responses** (varies by endpoint)
   - RESTful API responses
   - Configured with `[ResponseCache]` attribute

### Cache Invalidation

Caches are automatically cleared when data changes:

```csharp
// After creating/updating/deleting a vessel
_cacheService.Remove(CacheKeys.AllVessels);
_cacheService.Remove(CacheKeys.VesselsByStatus(vessel.CurrentStatus.ToString()));
_cacheService.Remove(CacheKeys.DashboardStats);
```

## Admin Dashboard

Access at `/Admin/Dashboard` (Admin role required)

### Features:
- **Cache Hit Rate**: Percentage of requests served from cache
- **Performance Metrics**: Operation timing statistics
- **Clear Cache**: Manual cache invalidation button
- **Slow Query Detection**: Automatically highlights operations > 1 second

### Cache Statistics Display:
- Total cache hits
- Total cache misses
- Overall hit rate percentage
- Historical performance data

## Configuration

### In `Program.cs`:

```csharp
// Memory cache
builder.Services.AddMemoryCache();

// Distributed cache (development - in-memory)
builder.Services.AddDistributedMemoryCache();

// Custom services
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();

// Response caching middleware
builder.Services.AddResponseCaching();
```

### Switching to Redis (Production):

```csharp
// Replace AddDistributedMemoryCache() with:
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "CargoTracker:";
});
```

### Switching to SQL Server Distributed Cache:

```csharp
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DistCache");
    options.SchemaName = "dbo";
    options.TableName = "CacheTable";
});
```

## Performance Benefits

### Without Caching:
- Vessel list query: ~50-100ms
- Each request hits the database
- High database load under traffic

### With Caching:
- First request: ~50-100ms (cache miss)
- Subsequent requests: <1ms (cache hit)
- 99%+ reduction in database queries
- Scales to thousands of requests/second

## Best Practices

### 1. Cache Expiration
- **Frequently Changing Data**: 1-5 minutes
- **Rarely Changing Data**: 10-30 minutes
- **Static Data**: 1 hour or more

### 2. Cache Keys
- Use constants from `CacheKeys` class
- Never hardcode cache key strings
- Use hierarchical naming: `entity:action:parameter`

### 3. Cache Invalidation
- Invalidate on Create, Update, Delete
- Clear related caches (e.g., all vessels + vessel by status)
- Consider cache dependencies for complex scenarios

### 4. Performance Monitoring
- Wrap expensive operations in `MeasureOperation`
- Monitor slow queries in Admin Dashboard
- Optimize operations that exceed thresholds

### 5. Error Handling
- Cache failures should not break the application
- Fall back to database on cache errors
- Log cache exceptions for monitoring

## Comparison with Other Frameworks

### vs. Django + Redis
**Django**:
- Requires Redis/Memcached setup
- Manual cache configuration
- Third-party packages needed

**.NET**:
- Built-in caching support
- No external dependencies for development
- Easy Redis integration for production

### vs. Node.js + node-cache
**Node.js**:
- Third-party libraries (node-cache, redis)
- Manual implementation
- Less type-safe

**.NET**:
- Framework-provided interfaces
- Strongly-typed generic cache
- Dependency injection built-in

## Monitoring in Production

### Application Insights Integration:
```csharp
// Track cache performance
telemetry.TrackMetric("Cache.HitRate", cacheStats.HitRate);
telemetry.TrackMetric("Cache.Hits", cacheStats.Hits);
telemetry.TrackMetric("Cache.Misses", cacheStats.Misses);
```

### Health Checks:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("cache");
```

## Advanced Features

### Sliding Expiration:
```csharp
var cacheOptions = new MemoryCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
    SlidingExpiration = TimeSpan.FromMinutes(2) // Reset timer on access
};
```

### Cache Dependencies:
```csharp
var cts = new CancellationTokenSource();
cacheOptions.AddExpirationToken(new CancellationChangeToken(cts.Token));
```

### Cache Priorities:
```csharp
cacheOptions.Priority = CacheItemPriority.High; // Less likely to be evicted
```

## Testing Cache Behavior

1. **First Load**: Navigate to Vessels page → Check database query time
2. **Cached Load**: Refresh page → Should be instant (< 1ms)
3. **Cache Invalidation**: Create new vessel → Cache cleared
4. **Next Load**: List reloads → New cache entry created
5. **Admin Dashboard**: View hit/miss statistics

## Summary

The caching system demonstrates:
- ✅ Built-in .NET caching (no external dependencies)
- ✅ Both in-memory and distributed cache support
- ✅ Automatic cache invalidation
- ✅ Performance monitoring with statistics
- ✅ Admin dashboard for monitoring
- ✅ Production-ready architecture
- ✅ Significant performance improvements

This is enterprise-grade caching without the complexity of managing Redis or other cache servers in development!
