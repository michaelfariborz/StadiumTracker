using System.ComponentModel.DataAnnotations;

namespace StadiumTracker.Data.Models;

public class League
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public required string Abbreviation { get; set; }

    public int SortOrder { get; set; }

    public ICollection<Stadium> Stadiums { get; set; } = [];
}
