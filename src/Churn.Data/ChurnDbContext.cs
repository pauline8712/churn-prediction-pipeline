using Churn.Core;
using Microsoft.EntityFrameworkCore;

namespace Churn.Data;

public class ChurnDbContext : DbContext
{
    public ChurnDbContext(DbContextOptions<ChurnDbContext> options)
        : base(options)
    {
    }

    // Each DbSet becomes a table
    public DbSet<User> Users => Set<User>();
    public DbSet<UserEvent> UserEvents => Set<UserEvent>();
    public DbSet<PredictionLog> PredictionLogs => Set<PredictionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite index: feature queries look up one user's events in a time range
        modelBuilder.Entity<UserEvent>()
            .HasIndex(e => new { e.UserId, e.TimestampUtc });

        // Monitoring queries look at the most recent predictions
        modelBuilder.Entity<PredictionLog>()
            .HasIndex(p => p.PredictedAtUtc);
    }
}