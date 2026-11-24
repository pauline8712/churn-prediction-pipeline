namespace Churn.Simulator;

public static class PersonaPicker
{
    // Picks a persona: 60% Active, 25% Declining, 15% Churning
    public static Persona Pick(Random random)
    {
        double roll = random.NextDouble(); // a number between 0.0 and 1.0

        if (roll < 0.60)
            return Persona.Active;     // 0.00 – 0.60 → 60%

        if (roll < 0.85)
            return Persona.Declining;  // 0.60 – 0.85 → 25%

        return Persona.Churning;       // 0.85 – 1.00 → 15%
    }
}