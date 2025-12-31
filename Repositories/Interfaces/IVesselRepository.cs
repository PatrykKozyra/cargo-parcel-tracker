using CargoParcelTracker.Models;

namespace CargoParcelTracker.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Vessel-specific operations
    /// </summary>
    public interface IVesselRepository : IRepository<Vessel>
    {
        Task<IEnumerable<Vessel>> GetVesselsByTypeAsync(VesselType vesselType);
        Task<IEnumerable<Vessel>> GetVesselsByStatusAsync(VesselStatus status);
        Task<Vessel?> GetVesselByImoNumberAsync(string imoNumber);
        Task<IEnumerable<Vessel>> GetAvailableVesselsAsync();
        Task<IEnumerable<Vessel>> GetVesselsWithVoyageAllocationsAsync();
        Task<Vessel?> GetVesselWithDetailsAsync(int id);
        Task<bool> IsImoNumberUniqueAsync(string imoNumber, int? excludeVesselId = null);
    }
}
