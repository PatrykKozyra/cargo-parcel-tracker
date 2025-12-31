using System.ComponentModel.DataAnnotations;

namespace CargoParcelTracker.Models
{
    public class Tanker
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Vessel Name")]
        public string VesselName { get; set; } = string.Empty;

        [Display(Name = "IMO Number")]
        public string? ImoNumber { get; set; }

        [Display(Name = "Capacity (Barrels)")]
        [Range(0, double.MaxValue)]
        public decimal? Capacity { get; set; }

        [Display(Name = "Flag Country")]
        public string? FlagCountry { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<CargoParcel> CargoParcels { get; set; } = new List<CargoParcel>();
    }
}
