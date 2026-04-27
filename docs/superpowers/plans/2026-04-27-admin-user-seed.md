# Admin User Seed Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Seed a default admin user from configuration on every startup so admin access is available on a fresh deploy without manual database intervention.

**Architecture:** A static `SeedAdminUserAsync` method is added to the `public partial class Program` block at the bottom of `Program.cs`. This keeps it in one file (no new files) while remaining testable from the test project. The startup block reads `AdminSeed:Email` and `AdminSeed:Password` from `IConfiguration` and calls the method; missing keys skip silently, failures throw.

**Tech Stack:** ASP.NET Core Identity (`UserManager<ApplicationUser>`, `RoleManager<IdentityRole>`), xunit, NSubstitute (not used here — real UserManager via SQLite in-memory), `Microsoft.Extensions.DependencyInjection`

---

### Task 1: Write failing tests for SeedAdminUserAsync

**Files:**
- Create: `StadiumTracker.Tests/AdminUserSeederTests.cs`

- [ ] **Step 1: Create the test file**

```csharp
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
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test StadiumTracker.Tests --filter "AdminUserSeederTests" -v minimal
```

Expected: compilation error — `Program.SeedAdminUserAsync` does not exist yet.

---

### Task 2: Implement SeedAdminUserAsync in Program.cs

**Files:**
- Modify: `StadiumTracker/Program.cs` (bottom of file, inside the existing `public partial class Program` block)

- [ ] **Step 1: Add the static method to the partial Program class**

Replace the existing bottom of `Program.cs`:
```csharp
public partial class Program { }  // expose to test project
```
with:
```csharp
public partial class Program
{
    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }
}
```

- [ ] **Step 2: Run tests to confirm they pass**

```bash
dotnet test StadiumTracker.Tests --filter "AdminUserSeederTests" -v minimal
```

Expected: 4 tests pass.

- [ ] **Step 3: Commit**

```bash
git add StadiumTracker/Program.cs StadiumTracker.Tests/AdminUserSeederTests.cs
git commit -m "feat: add SeedAdminUserAsync with tests"
```

---

### Task 3: Wire the seed call into startup

**Files:**
- Modify: `StadiumTracker/Program.cs` (startup block, after role seeding)

- [ ] **Step 1: Add the startup call after the role-seeding block**

In `Program.cs`, locate this block:
```csharp
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));
```

Add immediately after it:
```csharp
    var adminEmail = app.Configuration["AdminSeed:Email"];
    var adminPassword = app.Configuration["AdminSeed:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await Program.SeedAdminUserAsync(userManager, adminEmail, adminPassword);
    }
```

- [ ] **Step 2: Build to confirm no errors**

```bash
dotnet build StadiumTracker
```

Expected: Build succeeded, 0 errors.

- [ ] **Step 3: Set user secrets for local dev and do a smoke-test run**

```bash
dotnet user-secrets set "AdminSeed:Email" "admin@example.com" --project StadiumTracker
dotnet user-secrets set "AdminSeed:Password" "Admin123!" --project StadiumTracker
```

Then run the app and confirm it starts without errors:
```bash
dotnet run --project StadiumTracker
```

Expected: app starts, no `InvalidOperationException`, admin user visible in the Admin User Management page.

- [ ] **Step 4: Commit**

```bash
git add StadiumTracker/Program.cs
git commit -m "feat: seed admin user from configuration on startup"
```
