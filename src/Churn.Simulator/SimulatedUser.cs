namespace Churn.Simulator;

// In-memory description of a simulated user. Never stored in the database.
public record SimulatedUser(
    Persona Persona,
    DateTime SignupDateUtc,
    DateTime? ChurnDateUtc); // only set for Churning users