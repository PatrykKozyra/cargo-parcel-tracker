using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Data;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;

namespace CargoParcelTracker.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for VoyageAllocation entity
    /// </summary>
    public class VoyageAllocationRepository : Repository<VoyageAllocation>, IVoyageAllocationRepository
    {
        public VoyageAllocationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VoyageAllocation>> GetAllocationsByParcelIdAsync(int parcelId)
        {
            return await _dbSet
                .Include(va => va.Vessel)
                .Where(va => va.ParcelId == parcelId)
                .OrderBy(va => va.LoadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VoyageAllocation>> GetAllocationsByVesselIdAsync(int vesselId)
        {
            return await _dbSet
                .Include(va => va.CargoParcel)
                .Where(va => va.VesselId == vesselId)
                .OrderBy(va => va.LoadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VoyageAllocation>> GetAllocationsWithinDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(va => va.CargoParcel)
                .Include(va => va.Vessel)
                .Where(va => va.LoadingDate >= startDate && va.DischargeDate <= endDate)
                .OrderBy(va => va.LoadingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VoyageAllocation>> GetAllAllocationsWithDetailsAsync()
        {
            return await _dbSet
                .Include(va => va.CargoParcel)
                .Include(va => va.Vessel)
                .OrderByDescending(va => va.LoadingDate)
                .ToListAsync();
        }

        public async Task<VoyageAllocation?> GetAllocationWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(va => va.CargoParcel)
                .Include(va => va.Vessel)
                .FirstOrDefaultAsync(va => va.Id == id);
        }

        public async Task<decimal> GetTotalFreightRevenueAsync(int parcelId)
        {
            var allocations = await _dbSet
                .Include(va => va.CargoParcel)
                .Where(va => va.ParcelId == parcelId)
                .ToListAsync();

            return allocations.Sum(va => va.FreightRate * va.CargoParcel.QuantityBbls);
        }
    }
}
