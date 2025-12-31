using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoParcelTracker.Models
{
    public class VoyageAllocation
    {
        public int Id { get; set; }

        // Foreign Keys
        [Required]
        [Display(Name = "Cargo Parcel")]
        public int ParcelId { get; set; }

        [Required]
        [Display(Name = "Vessel")]
        public int VesselId { get; set; }

        [Required]
        [Display(Name = "Loading Date")]
        [DataType(DataType.DateTime)]
        public DateTime LoadingDate { get; set; }

        [Required]
        [Display(Name = "Discharge Date")]
        [DataType(DataType.DateTime)]
        public DateTime DischargeDate { get; set; }

        [Required]
        [Display(Name = "Freight Rate (USD/BBL)")]
        [Column(TypeName = "decimal(18,4)")]
        [Range(0, 999999)]
        public decimal FreightRate { get; set; }

        [Required]
        [Display(Name = "Demurrage Rate (USD/Day)")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999)]
        public decimal DemurrageRate { get; set; }

        // Navigation properties
        [ForeignKey("ParcelId")]
        public CargoParcel CargoParcel { get; set; } = null!;

        [ForeignKey("VesselId")]
        public Vessel Vessel { get; set; } = null!;
    }
}
