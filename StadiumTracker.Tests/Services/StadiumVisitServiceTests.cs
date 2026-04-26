using StadiumTracker.Data;
using StadiumTracker.Data.Models;
using StadiumTracker.Services;
using Xunit;

namespace StadiumTracker.Tests.Services;

public class StadiumVisitServiceTests
{
    private static ApplicationDbContext SeedStadium(out int stadiumId)
    {
        var ctx = TestDb.CreateFreshContext();
        var league = new League { Name = "MLB", Abbreviation = "MLB", SortOrder = 1 };
        var stadium = new Stadium
        {
            League = league, Name = "Fenway Park", HomeTeam = "Red Sox",
            City = "Boston", State = "MA", Latitude = 42.3467, Longitude = -71.0972
        };
        ctx.Stadiums.Add(stadium);
        ctx.SaveChanges();
        stadiumId = stadium.Id;
        return ctx;
    }

    [Fact]
    public async Task AddVisitAsync_PersistsAllFields()
    {
        using var ctx = SeedStadium(out var stadiumId);
        var svc = new StadiumVisitService(ctx);
        var date = new DateTime(2024, 7, 4);

        await svc.AddVisitAsync(stadiumId, "user-1", date, "NY Yankees", "3-2");

        var visit = ctx.StadiumVisits.Single();
        Assert.Equal(stadiumId, visit.StadiumId);
        Assert.Equal("user-1", visit.UserId);
        Assert.Equal(date, visit.VisitDate);
        Assert.Equal("NY Yankees", visit.OpponentTeam);
        Assert.Equal("3-2", visit.Score);
    }

    [Fact]
    public async Task GetVisitsForUserAsync_ReturnsOnlyCurrentUsersVisits()
    {
        using var ctx = SeedStadium(out var stadiumId);
        ctx.StadiumVisits.AddRange(
            new StadiumVisit { StadiumId = stadiumId, UserId = "user-1" },
            new StadiumVisit { StadiumId = stadiumId, UserId = "user-2" }
        );
        await ctx.SaveChangesAsync();
        var svc = new StadiumVisitService(ctx);

        var result = await svc.GetVisitsForUserAsync("user-1");

        Assert.Single(result);
        Assert.Equal("user-1", result[0].UserId);
    }

    [Fact]
    public async Task DeleteVisitAsync_ReturnsFalse_WhenVisitBelongsToOtherUser()
    {
        using var ctx = SeedStadium(out var stadiumId);
        var visit = new StadiumVisit { StadiumId = stadiumId, UserId = "user-1" };
        ctx.StadiumVisits.Add(visit);
        await ctx.SaveChangesAsync();
        var svc = new StadiumVisitService(ctx);

        var result = await svc.DeleteVisitAsync(visit.Id, "user-2");

        Assert.False(result);
        Assert.Equal(1, ctx.StadiumVisits.Count());
    }

    [Fact]
    public async Task UpdateVisitAsync_UpdatesFields()
    {
        using var ctx = SeedStadium(out var stadiumId);
        var visit = new StadiumVisit { StadiumId = stadiumId, UserId = "user-1", Score = "1-0" };
        ctx.StadiumVisits.Add(visit);
        await ctx.SaveChangesAsync();
        var svc = new StadiumVisitService(ctx);

        var result = await svc.UpdateVisitAsync(visit.Id, "user-1", new DateTime(2024, 8, 1), "Yankees", "5-3");

        Assert.True(result);
        var updated = ctx.StadiumVisits.Single();
        Assert.Equal("5-3", updated.Score);
        Assert.Equal("Yankees", updated.OpponentTeam);
    }
}
