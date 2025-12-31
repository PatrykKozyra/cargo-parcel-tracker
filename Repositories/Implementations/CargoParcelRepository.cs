using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Data;
using CargoParcelTracker.Models;
using CargoParcelTracker.Repositories.Interfaces;

namespace CargoParcelTracker.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for CargoParcel entity
    /// </summary>
    public class CargoParcelRepository : Repository<CargoParcel>, ICargoParcelRepository
    {
        public CargoParcelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CargoParcel>> GetParcelsByStatusAsync(CargoParcelStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CargoParcel>> GetParcelsWithinLaycanAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => p.LaycanStart >= startDate && p.LaycanEnd <= endDate)
                .OrderBy(p => p.LaycanStart)
                .ToListAsync();
        }

        public async Task<IEnumerable<CargoParcel>> GetParcelsByCrudeGradeAsync(string crudeGrade)
        {
            return await _dbSet
                .Where(p => p.CrudeGrade.ToLower() == crudeGrade.ToLower())
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<CargoParcel>> GetParcelsWithVoyageAllocationsAsync()
        {
            return await _dbSet
                .Include(p => p.VoyageAllocations)
                    .ThenInclude(va => va.Vessel)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<CargoParcel?> GetParcelWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.VoyageAllocations)
                    .ThenInclude(va => va.Vessel)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<decimal> GetTotalQuantityByStatusAsync(CargoParcelStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .SumAsync(p => p.QuantityBbls);
        }
    }
}
