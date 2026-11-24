using Churn.Core;

namespace Churn.Simulator;

// Creates one simulated user together with all of their events
public static class UserGenerator
{
    public static (User User, Persona Persona) Generate(
        Random random, DateTime simulationStartUtc, DateTime simulationEndUtc)
    {
        // Users sign up at some point during the first 90 days
        var signup = simulationStartUtc.AddDays(random.Next(0, 90));
        var persona = PersonaPicker.Pick(random);

        // Churners stop between 30 days after signup and 10 days before the end
        DateTime? churnDate = null;
        if (persona == Persona.Churning)
        {
            var earliest = signup.AddDays(30);
            var latest = simulationEndUtc.AddDays(-10);
            int range = (int)(latest - earliest).TotalDays;
            churnDate = earliest.AddDays(random.Next(0, range + 1));
        }

        var simulated = new SimulatedUser(persona, signup, churnDate);
        var user = new User { SignupDateUtc = signup };

        // Walk through every day and roll the dice
        for (var day = signup; day < simulationEndUtc; day = day.AddDays(1))
        {
            double probability = ActivityModel.DailyProbability(simulated, day, simulationEndUtc);
            if (random.NextDouble() < probability)
                AddSession(user, random, day);
        }

        return (user, persona);
    }

    // One visit: a login, a few page views and sometimes a purchase
    private static void AddSession(User user, Random random, DateTime dayUtc)
    {
        var time = dayUtc.AddMinutes(random.Next(0, 23 * 60));
        user.Events.Add(new UserEvent { Type = EventType.Login, TimestampUtc = time });

        int pageViews = random.Next(1, 6); // 1–5 page views
        for (int i = 0; i < pageViews; i++)
        {
            time = time.AddMinutes(random.Next(1, 10));
            user.Events.Add(new UserEvent { Type = EventType.PageView, TimestampUtc = time });
        }

        if (random.NextDouble() < 0.10) // 10% of visits end with a purchase
        {
            time = time.AddMinutes(random.Next(1, 5));
            user.Events.Add(new UserEvent { Type = EventType.Purchase, TimestampUtc = time });
        }
    }
}