using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public interface IStadiumService
{
    Task<List<Stadium>> GetStadiumsByLeagueWithUserVisitsAsync(int leagueId, string userId);
    Task<List<Stadium>> GetStadiumsByLeagueAsync(int leagueId);
    Task<List<Stadium>> GetAllStadiumsAsync();
    Task AddStadiumAsync(int leagueId, string name, string homeTeam, string city,
                         string state, double latitude, double longitude,
                         string? description = null);
}
