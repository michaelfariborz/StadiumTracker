using System.ComponentModel.DataAnnotations;

namespace StadiumTracker.Data.Models;

public class Stadium
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    [MaxLength(150)]
    public required string Name { get; set; }

    [MaxLength(150)]
    public required string HomeTeam { get; set; }

    [MaxLength(100)]
    public required string City { get; set; }

    [MaxLength(50)]
    public required string State { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string? Description { get; set; }

    public ICollection<StadiumVisit> Visits { get; set; } = [];
}
