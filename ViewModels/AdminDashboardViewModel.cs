using CargoParcelTracker.Services;

namespace CargoParcelTracker.ViewModels
{
    public class AdminDashboardViewModel
    {
        public CacheStatistics CacheStatistics { get; set; } = new();
        public PerformanceStatistics PerformanceStatistics { get; set; } = new();
    }
}
