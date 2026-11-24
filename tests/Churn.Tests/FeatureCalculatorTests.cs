using Churn.Core;

namespace Churn.Tests;

public class FeatureCalculatorTests
{
    private static readonly DateTime Cutoff = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Signup = Cutoff.AddDays(-60);

    // Helper to create an event a number of days relative to the cutoff (negative = before)
    private static UserEvent Event(EventType type, double daysFromCutoff) =>
        new() { Type = type, TimestampUtc = Cutoff.AddDays(daysFromCutoff) };

    private static UserFeatures Calculate(params UserEvent[] events) =>
        FeatureCalculator.Calculate(userId: 1, Signup, events, Cutoff);

    [Fact]
    public void EventsAfterCutoff_DoNotChangeAnyFeature()
    {
        // The most important test: proves there is no data leakage
        var past = new[]
        {
            Event(EventType.Login, -10),
            Event(EventType.PageView, -10),
            Event(EventType.Login, -2),
            Event(EventType.Purchase, -2),
        };
        var future = new[]
        {
            Event(EventType.Login, 1),
            Event(EventType.Purchase, 5),
            Event(EventType.PageView, 20),
        };

        var withoutFuture = Calculate(past);
        var withFuture = Calculate(past.Concat(future).ToArray());

        // Everything must be identical except the label
        Assert.Equal(withoutFuture with { Churned = withFuture.Churned }, withFuture);
    }

    [Fact]
    public void NoEventsAfterCutoff_IsChurned()
    {
        var result = Calculate(Event(EventType.Login, -5));

        Assert.True(result.Churned);
    }

    [Fact]
    public void EventWithinLabelWindow_IsNotChurned()
    {
        var result = Calculate(Event(EventType.Login, -5), Event(EventType.Login, 10));

        Assert.False(result.Churned);
    }

    [Fact]
    public void EventAfterLabelWindow_IsStillChurned()
    {
        // Coming back after 31 days is too late
        var result = Calculate(Event(EventType.Login, -5), Event(EventType.Login, 31));

        Assert.True(result.Churned);
    }

    [Fact]
    public void EventExactlyAtCutoff_CountsAsAfterNotBefore()
    {
        var result = Calculate(Event(EventType.Login, 0));

        Assert.Equal(0, result.SessionsLast7Days);
        Assert.False(result.Churned);
    }

    [Fact]
    public void DaysSinceLastEvent_IsMeasuredFromCutoff()
    {
        var result = Calculate(Event(EventType.Login, -20), Event(EventType.Login, -3));

        Assert.Equal(3f, result.DaysSinceLastEvent);
    }

    [Fact]
    public void ActivityTrend_IsNegative_WhenUserSlowsDown()
    {
        // 3 sessions in the previous week, 1 in the last week
        var result = Calculate(
            Event(EventType.Login, -12),
            Event(EventType.Login, -10),
            Event(EventType.Login, -9),
            Event(EventType.Login, -2));

        Assert.Equal(-2f, result.ActivityTrend);
    }

    [Fact]
    public void UserWithNoEvents_HasDaysSinceLastEventEqualToAccountAge()
    {
        var result = Calculate();

        Assert.Equal(60f, result.DaysSinceLastEvent);
        Assert.Equal(60f, result.AccountAgeDays);
        Assert.True(result.Churned);
    }
}