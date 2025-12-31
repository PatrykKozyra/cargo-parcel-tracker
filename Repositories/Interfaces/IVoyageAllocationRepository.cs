using CargoParcelTracker.Models;

namespace CargoParcelTracker.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for VoyageAllocation-specific operations
    /// </summary>
    public interface IVoyageAllocationRepository : IRepository<VoyageAllocation>
    {
        Task<IEnumerable<VoyageAllocation>> GetAllocationsByParcelIdAsync(int parcelId);
        Task<IEnumerable<VoyageAllocation>> GetAllocationsByVesselIdAsync(int vesselId);
        Task<IEnumerable<VoyageAllocation>> GetAllocationsWithinDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<VoyageAllocation>> GetAllAllocationsWithDetailsAsync();
        Task<VoyageAllocation?> GetAllocationWithDetailsAsync(int id);
        Task<decimal> GetTotalFreightRevenueAsync(int parcelId);
    }
}
