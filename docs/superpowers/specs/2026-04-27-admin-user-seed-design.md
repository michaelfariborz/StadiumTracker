# Admin User Seed Design

**Date:** 2026-04-27  
**Status:** Approved

## Problem

`Program.cs` seeds the "Admin" role on startup but never creates an admin user, making it impossible to access admin features on a fresh deploy without manual database intervention.

## Solution

Add a config-driven admin user seed block to `Program.cs` that runs on every startup (all environments). If the configured user doesn't exist it is created; if it exists but lacks the Admin role it is promoted. If the config keys are absent the block is silently skipped.

## Configuration

| Key | Description |
|-----|-------------|
| `AdminSeed:Email` | Email address (and username) for the admin account |
| `AdminSeed:Password` | Password — must satisfy the app's policy (8+ chars, uppercase, digit) |

Both keys must be present for the seed to run. Missing either silently skips the block.

**Dev (user secrets):**
```
dotnet user-secrets set "AdminSeed:Email" "admin@example.com" --project StadiumTracker
dotnet user-secrets set "AdminSeed:Password" "Admin123!" --project StadiumTracker
```

**Prod (environment variables):**
```
AdminSeed__Email=admin@example.com
AdminSeed__Password=Admin123!
```

## Seeding Logic

Location: `Program.cs`, after the existing role-seeding block.

1. Read `AdminSeed:Email` and `AdminSeed:Password` from `IConfiguration`
2. If either is null or whitespace, skip
3. Use `UserManager<ApplicationUser>` to find the user by email
4. If user does not exist: call `CreateAsync`, throw on failure
5. If user is not in the Admin role: call `AddToRoleAsync`, throw on failure

The block is idempotent — re-deploys do not create duplicate users or throw errors when the user already has the Admin role.

## Error Handling

Failures in `CreateAsync` or `AddToRoleAsync` (e.g. password does not meet policy) throw an `InvalidOperationException` with the EF Identity error messages included, so misconfiguration is loud at startup rather than silently ignored.

## Scope

- **In scope:** One seed block added to `Program.cs`. No new files.
- **Out of scope:** UI for rotating admin credentials, multiple seeded admin accounts, removing the admin role from a demoted user.
