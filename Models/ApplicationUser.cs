using Microsoft.AspNetCore.Identity;

namespace CargoParcelTracker.Models;

/// <summary>
/// Custom application user extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
