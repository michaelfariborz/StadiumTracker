using StadiumTracker.Data;
using StadiumTracker.Data.Models;
using StadiumTracker.Services;
using Xunit;

namespace StadiumTracker.Tests.Services;

public class StadiumServiceTests
{
    private static (ApplicationDbContext ctx, League mlb) SeedLeagueAndStadiums()
    {
        var ctx = TestDb.CreateFreshContext();
        var mlb = new League { Name = "MLB", Abbreviation = "MLB", SortOrder = 1 };
        var nfl = new League { Name = "NFL", Abbreviation = "NFL", SortOrder = 2 };
        ctx.Leagues.AddRange(mlb, nfl);
        ctx.Stadiums.AddRange(
            new Stadium { League = mlb, Name = "Fenway Park", HomeTeam = "Red Sox", City = "Boston", State = "MA", Latitude = 42.3467, Longitude = -71.0972 },
            new Stadium { League = nfl, Name = "Gillette Stadium", HomeTeam = "Patriots", City = "Foxborough", State = "MA", Latitude = 42.0909, Longitude = -71.2643 }
        );
        ctx.SaveChanges();
        return (ctx, mlb);
    }

    [Fact]
    public async Task GetStadiumsByLeagueAsync_FiltersToCorrectLeague()
    {
        var (ctx, mlb) = SeedLeagueAndStadiums();
        using (ctx)
        {
            var svc = new StadiumService(ctx);
            var result = await svc.GetStadiumsByLeagueAsync(mlb.Id);
            Assert.Single(result);
            Assert.Equal("Fenway Park", result[0].Name);
        }
    }

    [Fact]
    public async Task GetStadiumsByLeagueWithUserVisitsAsync_OnlyIncludesRequestingUsersVisits()
    {
        var (ctx, mlb) = SeedLeagueAndStadiums();
        using (ctx)
        {
            var stadium = ctx.Stadiums.First(s => s.Name == "Fenway Park");
            ctx.StadiumVisits.AddRange(
                new StadiumVisit { StadiumId = stadium.Id, UserId = "user-a", VisitDate = DateTime.Today },
                new StadiumVisit { StadiumId = stadium.Id, UserId = "user-b", VisitDate = DateTime.Today }
            );
            ctx.SaveChanges();

            var svc = new StadiumService(ctx);
            var result = await svc.GetStadiumsByLeagueWithUserVisitsAsync(mlb.Id, "user-a");

            Assert.Single(result);
            Assert.Single(result[0].Visits);
            Assert.Equal("user-a", result[0].Visits.First().UserId);
        }
    }

    [Fact]
    public async Task AddStadiumAsync_PersistsStadium()
    {
        var (ctx, mlb) = SeedLeagueAndStadiums();
        using (ctx)
        {
            var svc = new StadiumService(ctx);
            await svc.AddStadiumAsync(mlb.Id, "Yankee Stadium", "NY Yankees", "Bronx", "NY", 40.8296, -73.9262);
            Assert.Equal(2, ctx.Stadiums.Count(s => s.LeagueId == mlb.Id));
        }
    }
}
