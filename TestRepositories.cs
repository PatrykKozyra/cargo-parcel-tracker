using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Data;
using CargoParcelTracker.Repositories.Implementations;
using CargoParcelTracker.Repositories.Interfaces;

namespace CargoParcelTracker
{
    /// <summary>
    /// Test class to verify repository implementation and seeded data
    /// </summary>
    public static class RepositoryTester
    {
        public static async Task TestRepositoriesAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=cargo-tracker.db");

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            // Test repositories
            IVesselRepository vesselRepo = new VesselRepository(context);
            ICargoParcelRepository parcelRepo = new CargoParcelRepository(context);
            IVoyageAllocationRepository allocationRepo = new VoyageAllocationRepository(context);

            Console.WriteLine("=== Repository Pattern Test Results ===\n");

            // Test Vessel Repository
            var vesselCount = await vesselRepo.CountAsync();
            var availableVessels = await vesselRepo.GetAvailableVesselsAsync();
            var vlccVessels = await vesselRepo.GetVesselsByTypeAsync(Models.VesselType.VLCC);

            Console.WriteLine($"Total Vessels: {vesselCount}");
            Console.WriteLine($"Available Vessels: {availableVessels.Count()}");
            Console.WriteLine($"VLCC Vessels: {vlccVessels.Count()}");

            // Test Cargo Parcel Repository
            var parcelCount = await parcelRepo.CountAsync();
            var plannedParcels = await parcelRepo.GetParcelsByStatusAsync(Models.CargoParcelStatus.Planned);
            var totalQuantity = await parcelRepo.GetTotalQuantityByStatusAsync(Models.CargoParcelStatus.Planned);

            Console.WriteLine($"\nTotal Cargo Parcels: {parcelCount}");
            Console.WriteLine($"Planned Parcels: {plannedParcels.Count()}");
            Console.WriteLine($"Total Planned Quantity: {totalQuantity:N0} barrels");

            // Test Voyage Allocation Repository
            var allocationCount = await allocationRepo.CountAsync();
            var allocationsWithDetails = await allocationRepo.GetAllAllocationsWithDetailsAsync();

            Console.WriteLine($"\nTotal Voyage Allocations: {allocationCount}");
            Console.WriteLine($"Allocations with Details loaded: {allocationsWithDetails.Count()}");

            // Display sample data
            Console.WriteLine("\n=== Sample Vessels (First 5) ===");
            var sampleVessels = (await vesselRepo.GetAllAsync()).Take(5);
            foreach (var vessel in sampleVessels)
            {
                Console.WriteLine($"- {vessel.VesselName} ({vessel.ImoNumber}) - {vessel.VesselType} - {vessel.Dwt:N0} DWT");
            }

            Console.WriteLine("\n=== Sample Cargo Parcels (First 5) ===");
            var sampleParcels = (await parcelRepo.GetAllAsync()).Take(5);
            foreach (var parcel in sampleParcels)
            {
                Console.WriteLine($"- {parcel.ParcelName}: {parcel.CrudeGrade} - {parcel.QuantityBbls:N0} bbls");
                Console.WriteLine($"  {parcel.LoadingPort} → {parcel.DischargePort}");
            }

            Console.WriteLine("\n=== Sample Voyage Allocations (First 3) ===");
            var sampleAllocations = allocationsWithDetails.Take(3);
            foreach (var allocation in sampleAllocations)
            {
                Console.WriteLine($"- Vessel: {allocation.Vessel.VesselName}");
                Console.WriteLine($"  Parcel: {allocation.CargoParcel.ParcelName}");
                Console.WriteLine($"  Freight Rate: ${allocation.FreightRate:N4}/bbl");
                Console.WriteLine($"  Loading: {allocation.LoadingDate:yyyy-MM-dd}");
                Console.WriteLine();
            }

            Console.WriteLine("=== All Tests Completed Successfully! ===");
        }
    }
}
