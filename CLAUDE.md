# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the application (from repo root)
dotnet run --project StadiumTracker/StadiumTracker.csproj

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~AdminUserSeederTests"

# Run a single test method
dotnet test --filter "FullyQualifiedName~StadiumServiceTests.AddStadiumAsync_PersistsStadium"

# Add a new EF migration
dotnet ef migrations add <MigrationName> --project StadiumTracker/StadiumTracker.csproj

# Apply migrations
dotnet ef database update --project StadiumTracker/StadiumTracker.csproj
```

## Architecture

**StadiumTracker** is a .NET 10 Blazor Server application (`InteractiveServer` render mode) with ASP.NET Core Identity for authentication.

### Data Layer

- **Database**: SQL Server in production; SQLite in-memory for tests (`TestDb.CreateFreshContext()`)
- **`ApplicationDbContext`** extends `IdentityDbContext<ApplicationUser>` and owns `Leagues`, `Stadiums`, and `StadiumVisits`
- **Connection string** is configured in `appsettings.json` (`DefaultConnection`); the dev connection string should be set via User Secrets (`dotnet user-secrets`)
- **Migrations** live in `StadiumTracker/Data/Migrations/`
- **Seed data** runs at startup in `Program.cs`: leagues and stadiums are seeded from `SeedData.cs` if the tables are empty; admin user is seeded from `AdminSeed:Email` / `AdminSeed:Password` config keys

### Domain Model

- `League` → many `Stadium`s (delete restricted)
- `Stadium` → many `StadiumVisit`s (delete cascades)
- `ApplicationUser` → many `StadiumVisit`s (delete cascades)
- `StadiumVisit` records a user's visit to a stadium with optional `VisitDate`, `OpponentTeam`, and `Score`

### Service Layer

Three scoped services sit between Razor components and the DB:

| Interface | Implementation |
|---|---|
| `ILeagueService` | `LeagueService` |
| `IStadiumService` | `StadiumService` |
| `IStadiumVisitService` | `StadiumVisitService` |

`GetStadiumsByLeagueWithUserVisitsAsync(leagueId, userId)` is the key query — it filters visits to only the requesting user's records.

### Roles & Auth

- All pages require authentication (`[Authorize]`)
- A single `"Admin"` role gates the admin screens under `Components/Pages/Admin/`
- The `Admin` role is created and optionally seeded at startup via `Program.SeedAdminUserAsync`

### Map Feature

The home page (`Components/Pages/Home.razor`) renders a Leaflet map:

- Leaflet CSS/JS is loaded from unpkg CDN in `Components/App.razor`
- `wwwroot/js/leafletInterop.js` exposes `window.leafletInterop` with `initMap`, `addPins`, `clearPins`, and `triggerAddVisit`
- Blazor calls into JS via `IJSRuntime`; JS calls back into Blazor via a `DotNetObjectReference` to invoke `[JSInvokable] OpenAddVisitFromMap(int stadiumId)`
- Map is initialized in `OnAfterRenderAsync(firstRender)` — Leaflet requires the DOM element to exist before initialization
- All local scripts/styles must use `@Assets["path"]` (not bare paths) — .NET 10's `MapStaticAssets()` serves fingerprinted URLs and bare paths are not guaranteed to resolve
- **Debugging tip**: a CDN SRI hash mismatch causes the browser to silently block the script (no JS error thrown); if the map is blank, check the browser console for SRI failures before investigating JS code

### Testing

- Test project: `StadiumTracker.Tests/`
- Test framework: xUnit + bUnit (for component tests) + NSubstitute (for mocking)
- Service tests use `TestDb.CreateFreshContext()` which creates a fresh SQLite in-memory DB per test
- Auth/seeder tests (`AdminUserSeederTests`) build a full `ServiceProvider` with Identity to test `Program.SeedAdminUserAsync` end-to-end
- **Important**: Tests use SQLite but the app uses SQL Server — `GETUTCDATE()` in `OnModelCreating` works in SQL Server but not SQLite; `EnsureCreated()` is used in tests (not migrations)
