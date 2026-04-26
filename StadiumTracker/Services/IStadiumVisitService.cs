using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public interface IStadiumVisitService
{
    Task AddVisitAsync(int stadiumId, string userId, DateTime? visitDate,
                       string? opponentTeam, string? score);
    Task<List<StadiumVisit>> GetVisitsForUserAsync(string userId);
    Task<List<StadiumVisit>> GetVisitsForUserByLeagueAsync(string userId, int leagueId);
    Task<bool> UpdateVisitAsync(int visitId, string userId, DateTime? visitDate,
                                string? opponentTeam, string? score);
    Task<bool> DeleteVisitAsync(int visitId, string userId);
}
