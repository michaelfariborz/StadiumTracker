using Microsoft.EntityFrameworkCore;
using StadiumTracker.Data;
using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public class StadiumService(ApplicationDbContext db) : IStadiumService
{
    public Task<List<Stadium>> GetStadiumsByLeagueWithUserVisitsAsync(int leagueId, string userId) =>
        db.Stadiums
          .AsNoTracking()
          .Where(s => s.LeagueId == leagueId)
          .Include(s => s.Visits.Where(v => v.UserId == userId))
          .OrderBy(s => s.Name)
          .ToListAsync();

    public Task<List<Stadium>> GetStadiumsByLeagueAsync(int leagueId) =>
        db.Stadiums
          .Where(s => s.LeagueId == leagueId)
          .OrderBy(s => s.Name)
          .ToListAsync();

    public Task<List<Stadium>> GetAllStadiumsAsync() =>
        db.Stadiums
          .Include(s => s.League)
          .OrderBy(s => s.League.SortOrder)
          .ThenBy(s => s.Name)
          .ToListAsync();

    public async Task AddStadiumAsync(int leagueId, string name, string homeTeam,
                                      string city, string state,
                                      double latitude, double longitude,
                                      string? description = null)
    {
        db.Stadiums.Add(new Stadium
        {
            LeagueId    = leagueId,
            Name        = name,
            HomeTeam    = homeTeam,
            City        = city,
            State       = state,
            Latitude    = latitude,
            Longitude   = longitude,
            Description = description
        });
        await db.SaveChangesAsync();
    }
}
