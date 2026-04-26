using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public interface ILeagueService
{
    Task<List<League>> GetAllLeaguesAsync();
    Task AddLeagueAsync(string name, string abbreviation, int sortOrder);
}
