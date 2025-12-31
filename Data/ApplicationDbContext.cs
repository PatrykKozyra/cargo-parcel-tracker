using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // New domain models
        public DbSet<CargoParcel> CargoParcels { get; set; }
        public DbSet<Vessel> Vessels { get; set; }
        public DbSet<VoyageAllocation> VoyageAllocations { get; set; }

        // Legacy models (can be removed if not needed)
        public DbSet<Port> Ports { get; set; }
        public DbSet<Tanker> Tankers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure CargoParcel
            modelBuilder.Entity<CargoParcel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.QuantityBbls)
                    .HasPrecision(18, 2);

                entity.Property(e => e.Status)
                    .HasConversion<string>();
            });

            // Configure Vessel
            modelBuilder.Entity<Vessel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ImoNumber)
                    .IsUnique();

                entity.Property(e => e.Dwt)
                    .HasPrecision(18, 2);

                entity.Property(e => e.VesselType)
                    .HasConversion<string>();

                entity.Property(e => e.CurrentStatus)
                    .HasConversion<string>();
            });

            // Configure VoyageAllocation
            modelBuilder.Entity<VoyageAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Relationship with CargoParcel
                entity.HasOne(va => va.CargoParcel)
                    .WithMany(cp => cp.VoyageAllocations)
                    .HasForeignKey(va => va.ParcelId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship with Vessel
                entity.HasOne(va => va.Vessel)
                    .WithMany(v => v.VoyageAllocations)
                    .HasForeignKey(va => va.VesselId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.FreightRate)
                    .HasPrecision(18, 4);

                entity.Property(e => e.DemurrageRate)
                    .HasPrecision(18, 2);
            });

            // Legacy Port and Tanker configurations (if still needed)
            modelBuilder.Entity<Port>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Tanker>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
