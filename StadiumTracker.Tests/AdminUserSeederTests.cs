using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using StadiumTracker.Data;

namespace StadiumTracker.Tests;

public class AdminUserSeederTests : IAsyncDisposable
{
    private readonly ServiceProvider _sp;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserSeederTests()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        _sp = services.BuildServiceProvider();

        var db = _sp.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();

        var roleManager = _sp.GetRequiredService<RoleManager<IdentityRole>>();
        roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();

        _userManager = _sp.GetRequiredService<UserManager<ApplicationUser>>();
    }

    public async ValueTask DisposeAsync() => await _sp.DisposeAsync();

    [Fact]
    public async Task CreatesUserAndAssignsAdminRole_WhenUserDoesNotExist()
    {
        await Program.SeedAdminUserAsync(_userManager, "admin@example.com", "Admin123!");

        var user = await _userManager.FindByEmailAsync("admin@example.com");
        Assert.NotNull(user);
        Assert.True(await _userManager.IsInRoleAsync(user, "Admin"));
    }

    [Fact]
    public async Task PromotesExistingUser_WhenNotInAdminRole()
    {
        var existing = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com" };
        await _userManager.CreateAsync(existing, "Admin123!");

        await Program.SeedAdminUserAsync(_userManager, "admin@example.com", "Admin123!");

        Assert.True(await _userManager.IsInRoleAsync(existing, "Admin"));
    }

    [Fact]
    public async Task IsIdempotent_WhenCalledTwice()
    {
        await Program.SeedAdminUserAsync(_userManager, "admin@example.com", "Admin123!");
        var ex = await Record.ExceptionAsync(
            () => Program.SeedAdminUserAsync(_userManager, "admin@example.com", "Admin123!"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task ThrowsInvalidOperationException_WhenPasswordDoesNotMeetPolicy()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Program.SeedAdminUserAsync(_userManager, "admin@example.com", "weak"));
    }
}
