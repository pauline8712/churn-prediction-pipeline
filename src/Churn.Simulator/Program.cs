using Churn.Data;
using Churn.Simulator;
using Microsoft.EntityFrameworkCore;

const int UserCount = 5000;
const int Seed = 42;
const int BatchSize = 250;

// Fixed dates (not DateTime.UtcNow) so every run produces exactly the same data
var simulationEnd = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
var simulationStart = simulationEnd.AddDays(-180);

var random = new Random(Seed);
using var db = new ChurnDbContextFactory().CreateDbContext(args);

// Create or update the tables (needed in a fresh Docker environment)
db.Database.Migrate();

// Start from an empty database and reset the ids, so reruns give identical results
Console.WriteLine("Clearing existing data...");
db.Database.ExecuteSqlRaw("TRUNCATE \"UserEvents\", \"Users\" RESTART IDENTITY;");

var personaCounts = new Dictionary<Persona, int>();
var eventCount = 0;

for (int i = 1; i <= UserCount; i++)
{
    var (user, persona) = UserGenerator.Generate(random, simulationStart, simulationEnd);
    db.Users.Add(user);

    personaCounts[persona] = personaCounts.GetValueOrDefault(persona) + 1;
    eventCount += user.Events.Count;

    // Save in batches to keep memory usage low
    if (i % BatchSize == 0)
    {
        db.SaveChanges();
        db.ChangeTracker.Clear();
        Console.WriteLine($"Saved {i}/{UserCount} users ({eventCount:N0} events)");
    }
}

db.SaveChanges();

Console.WriteLine();
Console.WriteLine($"Done: {UserCount} users, {eventCount:N0} events");
foreach (var (persona, count) in personaCounts)
    Console.WriteLine($"  {persona}: {count}");