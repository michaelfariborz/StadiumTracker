using System.ComponentModel.DataAnnotations;

namespace StadiumTracker.Data.Models;

public class StadiumVisit
{
    public int Id { get; set; }

    public int StadiumId { get; set; }
    public Stadium Stadium { get; set; } = null!;

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public DateTime? VisitDate { get; set; }

    [MaxLength(150)]
    public string? OpponentTeam { get; set; }

    [MaxLength(50)]
    public string? Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
