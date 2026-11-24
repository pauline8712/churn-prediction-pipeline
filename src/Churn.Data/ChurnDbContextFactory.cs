using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Churn.Data;

// Used only by the EF Core tools (dotnet ef) to create the context at design time
public class ChurnDbContextFactory : IDesignTimeDbContextFactory<ChurnDbContext>
{
    public ChurnDbContext CreateDbContext(string[] args)
    {
        // Read from environment variable, fall back to local Docker database
        var connectionString =
            Environment.GetEnvironmentVariable("CHURN_DB")
            ?? "Host=localhost;Port=5433;Database=churn;Username=churn;Password=churn";

        var options = new DbContextOptionsBuilder<ChurnDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ChurnDbContext(options);
    }
}