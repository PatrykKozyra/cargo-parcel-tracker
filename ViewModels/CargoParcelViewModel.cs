using System.ComponentModel.DataAnnotations;
using CargoParcelTracker.Models;

namespace CargoParcelTracker.ViewModels
{
    /// <summary>
    /// ViewModel for CargoParcel Create/Edit operations with custom validation
    /// </summary>
    public class CargoParcelViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Parcel name is required")]
        [StringLength(200, ErrorMessage = "Parcel name cannot exceed 200 characters")]
        [Display(Name = "Parcel Name")]
        public string ParcelName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Crude grade is required")]
        [StringLength(100, ErrorMessage = "Crude grade cannot exceed 100 characters")]
        [Display(Name = "Crude Grade")]
        public string CrudeGrade { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1000, 3000000, ErrorMessage = "Quantity must be between 1,000 and 3,000,000 barrels")]
        [Display(Name = "Quantity (Barrels)")]
        public decimal QuantityBbls { get; set; }

        [Required(ErrorMessage = "Loading port is required")]
        [StringLength(200, ErrorMessage = "Loading port cannot exceed 200 characters")]
        [Display(Name = "Loading Port")]
        public string LoadingPort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discharge port is required")]
        [StringLength(200, ErrorMessage = "Discharge port cannot exceed 200 characters")]
        [Display(Name = "Discharge Port")]
        public string DischargePort { get; set; } = string.Empty;

        [Required(ErrorMessage = "Laycan start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Laycan Start")]
        public DateTime LaycanStart { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Laycan end date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Laycan End")]
        public DateTime LaycanEnd { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public CargoParcelStatus Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Custom validation logic
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Laycan end must be after laycan start
            if (LaycanEnd <= LaycanStart)
            {
                yield return new ValidationResult(
                    "Laycan End must be after Laycan Start",
                    new[] { nameof(LaycanEnd) });
            }

            // Laycan dates should not be too far in the past
            if (LaycanStart < DateTime.Today.AddDays(-90))
            {
                yield return new ValidationResult(
                    "Laycan Start cannot be more than 90 days in the past",
                    new[] { nameof(LaycanStart) });
            }

            // Loading and discharge ports should be different
            if (!string.IsNullOrWhiteSpace(LoadingPort) &&
                !string.IsNullOrWhiteSpace(DischargePort) &&
                LoadingPort.Equals(DischargePort, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Loading Port and Discharge Port must be different",
                    new[] { nameof(DischargePort) });
            }
        }
    }
}
