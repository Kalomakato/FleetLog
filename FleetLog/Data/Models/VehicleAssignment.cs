using System.ComponentModel.DataAnnotations;

namespace FleetLog.Data.Models;

public class VehicleAssignment
{
    [Key]
    public int Id { get; set; }

    public int DriverId { get; set; }

    public virtual Driver Driver { get; set; } = null!;

    public int VehicleId { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}