using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using StadiumTracker.Data;

namespace StadiumTracker.Tests.Auth;

public class AdminUserSeederTests : IAsyncLifetime
{
    private ServiceProvider _sp = null!;
    private UserManager<ApplicationUser> _userManager = null!;

    public async Task InitializeAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        _sp = services.BuildServiceProvider();

        var db = _sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = _sp.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.CreateAsync(new IdentityRole("Admin"));

        _userManager = _sp.GetRequiredService<UserManager<ApplicationUser>>();
    }

    public async Task DisposeAsync() => await _sp.DisposeAsync();

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

        Assert.False(await _userManager.IsInRoleAsync(existing, "Admin"));

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

        var user = await _userManager.FindByEmailAsync("admin@example.com");
        Assert.NotNull(user);
        Assert.True(await _userManager.IsInRoleAsync(user, "Admin"));
    }

    [Fact]
    public async Task ThrowsInvalidOperationException_WhenPasswordDoesNotMeetPolicy()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => Program.SeedAdminUserAsync(_userManager, "admin@example.com", "weak"));
    }
}
