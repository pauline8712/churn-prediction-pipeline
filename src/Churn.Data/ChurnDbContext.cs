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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: add an index here (see question below)
    }
}