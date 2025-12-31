using System.ComponentModel.DataAnnotations;

namespace CargoParcelTracker.Models
{
    public class Port
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Port Name")]
        public string PortName { get; set; } = string.Empty;

        [Display(Name = "Port Code")]
        public string? PortCode { get; set; }

        [Required]
        public string Country { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
