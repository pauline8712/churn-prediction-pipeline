namespace Churn.Core;

public class User
{
    // Primary key (EF Core detects "Id" automatically)
    public int Id { get; set; }

    // When the user signed up, always stored in UTC
    public DateTime SignupDateUtc { get; set; }

    // Navigation property: all events belonging to this user
    public List<UserEvent> Events { get; set; } = new();
}