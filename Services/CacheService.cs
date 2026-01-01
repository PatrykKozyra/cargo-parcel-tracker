using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CargoParcelTracker.Services
{
    /// <summary>
    /// Centralized caching service that supports both in-memory and distributed caching.
    /// Demonstrates .NET's built-in caching capabilities without external dependencies.
    /// </summary>
    public interface ICacheService
    {
        T? Get<T>(string key);
        Task<T?> GetAsync<T>(string key);
        void Set<T>(string key, T value, TimeSpan? expiration = null);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        void Remove(string key);
        Task RemoveAsync(string key);
        CacheStatistics GetStatistics();
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<CacheService> _logger;

        // Cache statistics
        private long _hits = 0;
        private long _misses = 0;
        private readonly object _statsLock = new object();

        public CacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <summary>
        /// Get value from memory cache (synchronous, fast)
        /// </summary>
        public T? Get<T>(string key)
        {
            var value = _memoryCache.Get<T>(key);

            lock (_statsLock)
            {
                if (value != null)
                {
                    _hits++;
                    _logger.LogDebug("Cache HIT for key: {Key}", key);
                }
                else
                {
                    _misses++;
                    _logger.LogDebug("Cache MISS for key: {Key}", key);
                }
            }

            return value;
        }

        /// <summary>
        /// Get value from distributed cache (async, supports Redis/SQL Server)
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var bytes = await _distributedCache.GetAsync(key);

                if (bytes != null && bytes.Length > 0)
                {
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    var value = JsonSerializer.Deserialize<T>(json);

                    lock (_statsLock)
                    {
                        _hits++;
                    }

                    _logger.LogDebug("Distributed cache HIT for key: {Key}", key);
                    return value;
                }

                lock (_statsLock)
                {
                    _misses++;
                }

                _logger.LogDebug("Distributed cache MISS for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from distributed cache for key: {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// Set value in memory cache with optional expiration
        /// </summary>
        public void Set<T>(string key, T value, TimeSpan? expiration = null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug("Cached value for key: {Key}, Expiration: {Expiration}", key, expiration);
        }

        /// <summary>
        /// Set value in distributed cache (supports Redis, SQL Server, etc.)
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };

                await _distributedCache.SetAsync(key, bytes, options);
                _logger.LogDebug("Distributed cache set for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to distributed cache for key: {Key}", key);
            }
        }

        /// <summary>
        /// Remove value from memory cache
        /// </summary>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed cache key: {Key}", key);
        }

        /// <summary>
        /// Remove value from distributed cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Removed distributed cache key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from distributed cache for key: {Key}", key);
            }
        }

        /// <summary>
        /// Get cache hit/miss statistics
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            lock (_statsLock)
            {
                var total = _hits + _misses;
                var hitRate = total > 0 ? (_hits * 100.0 / total) : 0;

                return new CacheStatistics
                {
                    Hits = _hits,
                    Misses = _misses,
                    Total = total,
                    HitRate = hitRate
                };
            }
        }
    }

    /// <summary>
    /// Cache performance statistics
    /// </summary>
    public class CacheStatistics
    {
        public long Hits { get; set; }
        public long Misses { get; set; }
        public long Total { get; set; }
        public double HitRate { get; set; }
    }
}
