namespace CargoParcelTracker.Services
{
    /// <summary>
    /// Centralized cache key definitions to avoid magic strings
    /// </summary>
    public static class CacheKeys
    {
        public const string AllVessels = "vessels:all";
        public const string AllParcels = "parcels:all";
        public const string AllVoyageAllocations = "voyages:all";
        public const string DashboardStats = "dashboard:stats";

        public static string VesselById(int id) => $"vessel:{id}";
        public static string ParcelById(int id) => $"parcel:{id}";
        public static string VoyageById(int id) => $"voyage:{id}";

        public static string VesselsByStatus(string status) => $"vessels:status:{status}";
        public static string ParcelsByStatus(string status) => $"parcels:status:{status}";
    }
}
