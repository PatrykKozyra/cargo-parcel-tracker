using CargoParcelTracker.Models;

namespace CargoParcelTracker.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for CargoParcel-specific operations
    /// </summary>
    public interface ICargoParcelRepository : IRepository<CargoParcel>
    {
        Task<IEnumerable<CargoParcel>> GetParcelsByStatusAsync(CargoParcelStatus status);
        Task<IEnumerable<CargoParcel>> GetParcelsWithinLaycanAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CargoParcel>> GetParcelsByCrudeGradeAsync(string crudeGrade);
        Task<IEnumerable<CargoParcel>> GetParcelsWithVoyageAllocationsAsync();
        Task<CargoParcel?> GetParcelWithDetailsAsync(int id);
        Task<decimal> GetTotalQuantityByStatusAsync(CargoParcelStatus status);
    }
}
