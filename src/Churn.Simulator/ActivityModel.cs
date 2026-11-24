namespace Churn.Simulator;

// Decides how likely a user is to visit the site on a given day
public static class ActivityModel
{
    private const double HighActivity = 0.6; // about 4 days a week
    private const double LowActivity = 0.1;  // about once every 10 days
    private const int DeclineDays = 30;      // churners fade out over 30 days

    public static double DailyProbability(SimulatedUser user, DateTime dayUtc, DateTime simulationEndUtc)
    {
        // No activity before the user has signed up
        if (dayUtc < user.SignupDateUtc)
            return 0;

        switch (user.Persona)
        {
            case Persona.Active:
                return HighActivity;

            case Persona.Declining:
            {
                // How far through the user's lifetime we are: 0.0 at signup, 1.0 at the end
                double lifetime = (simulationEndUtc - user.SignupDateUtc).TotalDays;
                double progress = (dayUtc - user.SignupDateUtc).TotalDays / lifetime;
                return Lerp(HighActivity, LowActivity, progress);
            }

            case Persona.Churning:
            {
                var churnDate = user.ChurnDateUtc!.Value;

                // Gone for good after the churn date
                if (dayUtc >= churnDate)
                    return 0;

                double daysLeft = (churnDate - dayUtc).TotalDays;

                // Normal activity until the decline starts
                if (daysLeft > DeclineDays)
                    return HighActivity;

                // Fade from High to Low during the last 30 days
                return Lerp(LowActivity, HighActivity, daysLeft / DeclineDays);
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(user), "Unknown persona");
        }
    }

    // Linear interpolation: t = 0 gives 'from', t = 1 gives 'to'
    private static double Lerp(double from, double to, double t) => from + (to - from) * t;
}