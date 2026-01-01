using System.Diagnostics;

namespace CargoParcelTracker.Services
{
    /// <summary>
    /// Service to monitor and log database query performance
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        IDisposable MeasureOperation(string operationName);
        PerformanceStatistics GetStatistics();
    }

    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly List<PerformanceMetric> _metrics = new();
        private readonly object _metricsLock = new object();

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
        }

        public IDisposable MeasureOperation(string operationName)
        {
            return new OperationTimer(operationName, this, _logger);
        }

        internal void RecordMetric(string operationName, long elapsedMilliseconds)
        {
            lock (_metricsLock)
            {
                _metrics.Add(new PerformanceMetric
                {
                    OperationName = operationName,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });

                // Keep only last 1000 metrics
                if (_metrics.Count > 1000)
                {
                    _metrics.RemoveAt(0);
                }
            }
        }

        public PerformanceStatistics GetStatistics()
        {
            lock (_metricsLock)
            {
                if (_metrics.Count == 0)
                {
                    return new PerformanceStatistics();
                }

                var grouped = _metrics
                    .GroupBy(m => m.OperationName)
                    .Select(g => new OperationStats
                    {
                        OperationName = g.Key,
                        Count = g.Count(),
                        AverageMs = g.Average(m => m.ElapsedMilliseconds),
                        MinMs = g.Min(m => m.ElapsedMilliseconds),
                        MaxMs = g.Max(m => m.ElapsedMilliseconds)
                    })
                    .OrderByDescending(s => s.AverageMs)
                    .ToList();

                return new PerformanceStatistics
                {
                    TotalOperations = _metrics.Count,
                    OperationStats = grouped,
                    SlowestOperations = grouped.Take(10).ToList()
                };
            }
        }

        private class OperationTimer : IDisposable
        {
            private readonly string _operationName;
            private readonly PerformanceMonitoringService _service;
            private readonly ILogger _logger;
            private readonly Stopwatch _stopwatch;

            public OperationTimer(
                string operationName,
                PerformanceMonitoringService service,
                ILogger logger)
            {
                _operationName = operationName;
                _service = service;
                _logger = logger;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                var elapsed = _stopwatch.ElapsedMilliseconds;

                _service.RecordMetric(_operationName, elapsed);

                if (elapsed > 1000) // Warn if operation takes more than 1 second
                {
                    _logger.LogWarning(
                        "Slow operation detected: {Operation} took {Elapsed}ms",
                        _operationName,
                        elapsed
                    );
                }
                else
                {
                    _logger.LogDebug(
                        "Operation {Operation} completed in {Elapsed}ms",
                        _operationName,
                        elapsed
                    );
                }
            }
        }
    }

    public class PerformanceMetric
    {
        public string OperationName { get; set; } = string.Empty;
        public long ElapsedMilliseconds { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PerformanceStatistics
    {
        public int TotalOperations { get; set; }
        public List<OperationStats> OperationStats { get; set; } = new();
        public List<OperationStats> SlowestOperations { get; set; } = new();
    }

    public class OperationStats
    {
        public string OperationName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageMs { get; set; }
        public long MinMs { get; set; }
        public long MaxMs { get; set; }
    }
}
