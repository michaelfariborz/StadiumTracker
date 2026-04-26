using Microsoft.EntityFrameworkCore;
using StadiumTracker.Data;
using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public class LeagueService(ApplicationDbContext db) : ILeagueService
{
    public Task<List<League>> GetAllLeaguesAsync() =>
        db.Leagues
          .Include(l => l.Stadiums)
          .OrderBy(l => l.SortOrder)
          .ToListAsync();

    public async Task AddLeagueAsync(string name, string abbreviation, int sortOrder)
    {
        db.Leagues.Add(new League { Name = name, Abbreviation = abbreviation, SortOrder = sortOrder });
        await db.SaveChangesAsync();
    }
}
