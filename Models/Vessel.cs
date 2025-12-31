using System.ComponentModel.DataAnnotations;

namespace CargoParcelTracker.Models
{
    public class Vessel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vessel Name")]
        [StringLength(200)]
        public string VesselName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "IMO Number")]
        [StringLength(20)]
        [RegularExpression(@"^IMO\d{7}$", ErrorMessage = "IMO Number must be in format IMO followed by 7 digits")]
        public string ImoNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Deadweight Tonnage (DWT)")]
        [Range(0, 999999)]
        public decimal Dwt { get; set; }

        [Required]
        [Display(Name = "Vessel Type")]
        public VesselType VesselType { get; set; }

        [Required]
        [Display(Name = "Current Status")]
        public VesselStatus CurrentStatus { get; set; } = VesselStatus.Available;

        // Navigation properties
        public ICollection<VoyageAllocation> VoyageAllocations { get; set; } = new List<VoyageAllocation>();
    }
}
