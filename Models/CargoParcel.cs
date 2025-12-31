using System.ComponentModel.DataAnnotations;

namespace CargoParcelTracker.Models
{
    public class CargoParcel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Parcel Name")]
        [StringLength(200)]
        public string ParcelName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Crude Grade")]
        [StringLength(100)]
        public string CrudeGrade { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Quantity (Barrels)")]
        [Range(0, 999999999)]
        public decimal QuantityBbls { get; set; }

        [Required]
        [Display(Name = "Loading Port")]
        [StringLength(200)]
        public string LoadingPort { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Discharge Port")]
        [StringLength(200)]
        public string DischargePort { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Laycan Start")]
        [DataType(DataType.Date)]
        public DateTime LaycanStart { get; set; }

        [Required]
        [Display(Name = "Laycan End")]
        [DataType(DataType.Date)]
        public DateTime LaycanEnd { get; set; }

        [Required]
        [Display(Name = "Status")]
        public CargoParcelStatus Status { get; set; } = CargoParcelStatus.Planned;

        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<VoyageAllocation> VoyageAllocations { get; set; } = new List<VoyageAllocation>();
    }
}
