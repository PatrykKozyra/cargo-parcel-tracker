using Microsoft.EntityFrameworkCore;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Data
{
    /// <summary>
    /// Database seeder for populating initial test data
    /// </summary>
    public static class DbSeeder
    {
        private static readonly Random _random = new Random(42); // Fixed seed for consistent data

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // Check if data already exists
            if (await context.Vessels.AnyAsync())
            {
                return; // Database has already been seeded
            }

            // Seed Vessels (50 vessels)
            var vessels = GenerateVessels(50);
            await context.Vessels.AddRangeAsync(vessels);
            await context.SaveChangesAsync();

            // Seed Cargo Parcels (75 parcels)
            var cargoParcels = GenerateCargoParcels(75);
            await context.CargoParcels.AddRangeAsync(cargoParcels);
            await context.SaveChangesAsync();

            // Seed Voyage Allocations (approximately 100 allocations)
            var voyageAllocations = GenerateVoyageAllocations(vessels, cargoParcels, 100);
            await context.VoyageAllocations.AddRangeAsync(voyageAllocations);
            await context.SaveChangesAsync();
        }

        private static List<Vessel> GenerateVessels(int count)
        {
            var vessels = new List<Vessel>();
            var vesselTypes = Enum.GetValues<VesselType>();
            var vesselStatuses = Enum.GetValues<VesselStatus>();

            var vesselPrefixes = new[] { "MT", "MV", "VLCC" };
            var vesselNames = new[]
            {
                "Pacific", "Atlantic", "Ocean", "Marine", "Horizon", "Navigator", "Explorer",
                "Pioneer", "Victory", "Liberty", "Freedom", "Spirit", "Unity", "Destiny",
                "Enterprise", "Endeavour", "Discovery", "Triumph", "Majestic", "Noble",
                "Sovereign", "Imperial", "Royal", "Elite", "Premier", "Supreme", "Grand",
                "Golden", "Silver", "Diamond", "Platinum", "Crystal", "Pearl", "Sapphire",
                "Emerald", "Ruby", "Topaz", "Amber", "Jade", "Coral", "Aurora", "Stellar",
                "Celestial", "Galaxy", "Cosmos", "Nebula", "Polaris", "Sirius", "Vega"
            };

            for (int i = 0; i < count; i++)
            {
                var vesselType = vesselTypes[_random.Next(vesselTypes.Length)];
                var dwt = vesselType switch
                {
                    VesselType.VLCC => _random.Next(200000, 320000),
                    VesselType.Suezmax => _random.Next(120000, 200000),
                    VesselType.Aframax => _random.Next(80000, 120000),
                    VesselType.Panamax => _random.Next(60000, 80000),
                    VesselType.Handysize => _random.Next(30000, 60000),
                    VesselType.ProductTanker => _random.Next(20000, 50000),
                    _ => _random.Next(50000, 150000)
                };

                vessels.Add(new Vessel
                {
                    VesselName = $"{vesselPrefixes[_random.Next(vesselPrefixes.Length)]} {vesselNames[_random.Next(vesselNames.Length)]} {(i > 25 ? "II" : "")}",
                    ImoNumber = $"IMO{1000000 + i:D7}",
                    Dwt = dwt,
                    VesselType = vesselType,
                    CurrentStatus = vesselStatuses[_random.Next(vesselStatuses.Length)]
                });
            }

            return vessels;
        }

        private static List<CargoParcel> GenerateCargoParcels(int count)
        {
            var cargoParcels = new List<CargoParcel>();
            var statuses = Enum.GetValues<CargoParcelStatus>();

            var crudeGrades = new[]
            {
                "Brent Crude", "WTI (West Texas Intermediate)", "Dubai Crude", "Oman Crude",
                "Urals", "Bonny Light", "Arab Light", "Arab Heavy", "Basra Light",
                "Forcados", "Qua Iboe", "Escravos", "Brass River", "Pennington",
                "Alaska North Slope", "Maya", "Kirkuk", "Iranian Heavy", "Iranian Light",
                "Kuwait Export", "Murban", "Das Blend", "Upper Zakum", "Qatar Marine"
            };

            var ports = new[]
            {
                "Ras Tanura, Saudi Arabia", "Houston, USA", "Rotterdam, Netherlands",
                "Singapore", "Jebel Ali, UAE", "Fujairah, UAE", "Ningbo, China",
                "Shanghai, China", "Ulsan, South Korea", "Yokohama, Japan",
                "Mumbai, India", "Lagos, Nigeria", "Bonny, Nigeria",
                "Valdez, USA", "Corpus Christi, USA", "Galveston, USA",
                "Kharg Island, Iran", "Basra, Iraq", "Kuwait City, Kuwait",
                "Dampier, Australia", "Gladstone, Australia", "Port Harcourt, Nigeria"
            };

            for (int i = 0; i < count; i++)
            {
                var laycanStart = DateTime.UtcNow.AddDays(_random.Next(-30, 60));
                var laycanEnd = laycanStart.AddDays(_random.Next(3, 10));

                cargoParcels.Add(new CargoParcel
                {
                    ParcelName = $"PARCEL-{DateTime.UtcNow.Year}-{(i + 1):D4}",
                    CrudeGrade = crudeGrades[_random.Next(crudeGrades.Length)],
                    QuantityBbls = _random.Next(300000, 2000000),
                    LoadingPort = ports[_random.Next(ports.Length)],
                    DischargePort = ports[_random.Next(ports.Length)],
                    LaycanStart = laycanStart,
                    LaycanEnd = laycanEnd,
                    Status = statuses[_random.Next(statuses.Length)],
                    CreatedDate = DateTime.UtcNow.AddDays(-_random.Next(1, 90))
                });
            }

            return cargoParcels;
        }

        private static List<VoyageAllocation> GenerateVoyageAllocations(
            List<Vessel> vessels,
            List<CargoParcel> cargoParcels,
            int count)
        {
            var voyageAllocations = new List<VoyageAllocation>();

            // Ensure we don't exceed the number of parcels
            count = Math.Min(count, cargoParcels.Count);

            // Select random parcels to allocate
            var parcelsToAllocate = cargoParcels
                .OrderBy(x => _random.Next())
                .Take(count)
                .ToList();

            foreach (var parcel in parcelsToAllocate)
            {
                var vessel = vessels[_random.Next(vessels.Count)];

                var loadingDate = parcel.LaycanStart.AddDays(_random.Next(0, 3));
                var voyageDuration = _random.Next(10, 30);
                var dischargeDate = loadingDate.AddDays(voyageDuration);

                voyageAllocations.Add(new VoyageAllocation
                {
                    CargoParcel = parcel,
                    Vessel = vessel,
                    LoadingDate = loadingDate,
                    DischargeDate = dischargeDate,
                    FreightRate = (decimal)(_random.NextDouble() * 5 + 1), // $1-6 per barrel
                    DemurrageRate = _random.Next(15000, 50000) // $15k-50k per day
                });
            }

            return voyageAllocations;
        }
    }
}
