using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StadiumTracker.Data.Models;

namespace StadiumTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Stadium> Stadiums => Set<Stadium>();
    public DbSet<StadiumVisit> StadiumVisits => Set<StadiumVisit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<League>(e =>
        {
            e.HasIndex(l => l.Abbreviation).IsUnique();
        });

        builder.Entity<Stadium>(e =>
        {
            e.HasIndex(s => new { s.Name, s.LeagueId }).IsUnique();
            e.HasOne(s => s.League)
             .WithMany(l => l.Stadiums)
             .HasForeignKey(s => s.LeagueId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StadiumVisit>(e =>
        {
            e.HasOne(v => v.Stadium)
             .WithMany(s => s.Visits)
             .HasForeignKey(v => v.StadiumId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.User)
             .WithMany(u => u.StadiumVisits)
             .HasForeignKey(v => v.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(v => v.CreatedAt)
             .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
