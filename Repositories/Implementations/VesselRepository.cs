using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Data;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;

namespace CargoParcelTracker.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Vessel entity
    /// </summary>
    public class VesselRepository : Repository<Vessel>, IVesselRepository
    {
        public VesselRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vessel>> GetVesselsByTypeAsync(VesselType vesselType)
        {
            return await _dbSet
                .Where(v => v.VesselType == vesselType)
                .OrderBy(v => v.VesselName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vessel>> GetVesselsByStatusAsync(VesselStatus status)
        {
            return await _dbSet
                .Where(v => v.CurrentStatus == status)
                .OrderBy(v => v.VesselName)
                .ToListAsync();
        }

        public async Task<Vessel?> GetVesselByImoNumberAsync(string imoNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.ImoNumber == imoNumber);
        }

        public async Task<IEnumerable<Vessel>> GetAvailableVesselsAsync()
        {
            return await _dbSet
                .Where(v => v.CurrentStatus == VesselStatus.Available)
                .OrderBy(v => v.VesselName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vessel>> GetVesselsWithVoyageAllocationsAsync()
        {
            return await _dbSet
                .Include(v => v.VoyageAllocations)
                    .ThenInclude(va => va.CargoParcel)
                .OrderBy(v => v.VesselName)
                .ToListAsync();
        }

        public async Task<Vessel?> GetVesselWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(v => v.VoyageAllocations)
                    .ThenInclude(va => va.CargoParcel)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> IsImoNumberUniqueAsync(string imoNumber, int? excludeVesselId = null)
        {
            var query = _dbSet.Where(v => v.ImoNumber == imoNumber);

            if (excludeVesselId.HasValue)
            {
                query = query.Where(v => v.Id != excludeVesselId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
