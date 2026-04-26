using StadiumTracker.Data.Models;
using StadiumTracker.Services;
using Xunit;

namespace StadiumTracker.Tests.Services;

public class LeagueServiceTests
{
    [Fact]
    public async Task GetAllLeaguesAsync_ReturnsSortedBySortOrder()
    {
        using var ctx = TestDb.CreateFreshContext();
        ctx.Leagues.AddRange(
            new League { Name = "NBA", Abbreviation = "NBA", SortOrder = 2 },
            new League { Name = "MLB", Abbreviation = "MLB", SortOrder = 1 }
        );
        await ctx.SaveChangesAsync();
        var svc = new LeagueService(ctx);

        var result = await svc.GetAllLeaguesAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("MLB", result[0].Abbreviation);
        Assert.Equal("NBA", result[1].Abbreviation);
    }

    [Fact]
    public async Task AddLeagueAsync_PersistsLeague()
    {
        using var ctx = TestDb.CreateFreshContext();
        var svc = new LeagueService(ctx);

        await svc.AddLeagueAsync("Major League Baseball", "MLB", 1);

        Assert.Equal(1, ctx.Leagues.Count());
        Assert.Equal("MLB", ctx.Leagues.Single().Abbreviation);
    }
}
