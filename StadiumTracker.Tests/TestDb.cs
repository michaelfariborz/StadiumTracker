using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StadiumTracker.Data;

namespace StadiumTracker.Tests;

public static class TestDb
{
    public static ApplicationDbContext CreateFreshContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        return ctx;
    }
}
