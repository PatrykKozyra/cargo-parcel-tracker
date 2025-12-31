using System.ComponentModel.DataAnnotations;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.ViewModels
{
    /// <summary>
    /// ViewModel for Vessel Create/Edit operations with enhanced validation
    /// </summary>
    public class VesselViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vessel name is required")]
        [StringLength(200, ErrorMessage = "Vessel name cannot exceed 200 characters")]
        [Display(Name = "Vessel Name")]
        public string VesselName { get; set; } = string.Empty;

        [Required(ErrorMessage = "IMO number is required")]
        [RegularExpression(@"^IMO\d{7}$", ErrorMessage = "IMO Number must be in format IMO followed by 7 digits (e.g., IMO1234567)")]
        [Display(Name = "IMO Number")]
        public string ImoNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Deadweight tonnage is required")]
        [Range(1000, 500000, ErrorMessage = "DWT must be between 1,000 and 500,000")]
        [Display(Name = "Deadweight Tonnage (DWT)")]
        public decimal Dwt { get; set; }

        [Required(ErrorMessage = "Vessel type is required")]
        [Display(Name = "Vessel Type")]
        public VesselType VesselType { get; set; }

        [Required(ErrorMessage = "Current status is required")]
        [Display(Name = "Current Status")]
        public VesselStatus CurrentStatus { get; set; }
    }
}
