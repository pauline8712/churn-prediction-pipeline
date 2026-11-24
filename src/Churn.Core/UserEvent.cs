namespace Churn.Core;

public class UserEvent
{
     // Primary key, long because the events table grows fast
    public long Id { get; set; }

    // Foreign key to User (EF Core detects "UserId" automatically)
    public int UserId { get; set; }

    // Navigation property to the owning user, populated by EF Core
    public User User { get; set; } = null!;

    // What kind of event this is
    public EventType Type { get; set; }

    // When the event happened, always stored in UTC
    public DateTime TimestampUtc { get; set; }
}