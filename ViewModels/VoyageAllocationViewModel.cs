using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CargoParcelTracker.ViewModels
{
    /// <summary>
    /// ViewModel for VoyageAllocation Create/Edit with dropdown lists
    /// </summary>
    public class VoyageAllocationViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a cargo parcel")]
        [Display(Name = "Cargo Parcel")]
        public int ParcelId { get; set; }

        [Required(ErrorMessage = "Please select a vessel")]
        [Display(Name = "Vessel")]
        public int VesselId { get; set; }

        [Required(ErrorMessage = "Loading date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Loading Date")]
        public DateTime LoadingDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Discharge date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Discharge Date")]
        public DateTime DischargeDate { get; set; } = DateTime.Today.AddDays(15);

        [Required(ErrorMessage = "Freight rate is required")]
        [Range(0.1, 50, ErrorMessage = "Freight rate must be between $0.10 and $50.00 per barrel")]
        [Display(Name = "Freight Rate (USD/BBL)")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
        public decimal FreightRate { get; set; }

        [Required(ErrorMessage = "Demurrage rate is required")]
        [Range(1000, 100000, ErrorMessage = "Demurrage rate must be between $1,000 and $100,000 per day")]
        [Display(Name = "Demurrage Rate (USD/Day)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal DemurrageRate { get; set; }

        // For dropdown lists
        public IEnumerable<SelectListItem>? AvailableParcels { get; set; }
        public IEnumerable<SelectListItem>? AvailableVessels { get; set; }

        /// <summary>
        /// Custom validation for voyage allocation
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Discharge must be after loading
            if (DischargeDate <= LoadingDate)
            {
                yield return new ValidationResult(
                    "Discharge Date must be after Loading Date",
                    new[] { nameof(DischargeDate) });
            }

            // Voyage duration validation (minimum 3 days, maximum 60 days)
            var voyageDuration = (DischargeDate - LoadingDate).TotalDays;
            if (voyageDuration < 3)
            {
                yield return new ValidationResult(
                    "Voyage duration must be at least 3 days",
                    new[] { nameof(DischargeDate) });
            }

            if (voyageDuration > 60)
            {
                yield return new ValidationResult(
                    "Voyage duration cannot exceed 60 days",
                    new[] { nameof(DischargeDate) });
            }
        }
    }
}
