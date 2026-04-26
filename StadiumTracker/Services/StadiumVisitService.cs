using Microsoft.EntityFrameworkCore;
using StadiumTracker.Data;
using StadiumTracker.Data.Models;

namespace StadiumTracker.Services;

public class StadiumVisitService(ApplicationDbContext db) : IStadiumVisitService
{
    public async Task AddVisitAsync(int stadiumId, string userId, DateTime? visitDate,
                                    string? opponentTeam, string? score)
    {
        db.StadiumVisits.Add(new StadiumVisit
        {
            StadiumId    = stadiumId,
            UserId       = userId,
            VisitDate    = visitDate,
            OpponentTeam = opponentTeam,
            Score        = score
        });
        await db.SaveChangesAsync();
    }

    public Task<List<StadiumVisit>> GetVisitsForUserAsync(string userId) =>
        db.StadiumVisits
          .Include(v => v.Stadium).ThenInclude(s => s.League)
          .Where(v => v.UserId == userId)
          .OrderByDescending(v => v.VisitDate ?? v.CreatedAt)
          .ToListAsync();

    public Task<List<StadiumVisit>> GetVisitsForUserByLeagueAsync(string userId, int leagueId) =>
        db.StadiumVisits
          .Include(v => v.Stadium)
          .Where(v => v.UserId == userId && v.Stadium.LeagueId == leagueId)
          .OrderByDescending(v => v.VisitDate ?? v.CreatedAt)
          .ToListAsync();

    public async Task<bool> UpdateVisitAsync(int visitId, string userId, DateTime? visitDate,
                                             string? opponentTeam, string? score)
    {
        var visit = await db.StadiumVisits
            .FirstOrDefaultAsync(v => v.Id == visitId && v.UserId == userId);
        if (visit is null) return false;
        visit.VisitDate    = visitDate;
        visit.OpponentTeam = opponentTeam;
        visit.Score        = score;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVisitAsync(int visitId, string userId)
    {
        var visit = await db.StadiumVisits
            .FirstOrDefaultAsync(v => v.Id == visitId && v.UserId == userId);
        if (visit is null) return false;
        db.StadiumVisits.Remove(visit);
        await db.SaveChangesAsync();
        return true;
    }
}
