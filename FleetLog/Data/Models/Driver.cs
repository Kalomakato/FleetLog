using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FleetLog.Data.Models;

public class Driver
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public virtual IdentityUser User { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<VehicleAssignment> VehicleAssignments { get; set; }
        = new List<VehicleAssignment>();
}