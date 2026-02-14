using WinCalendar.Domain.Entities;

namespace WinCalendar.Tests;

public class CountdownCardPresentationTests
{
    [Fact]
    public void BuildPresentation_Should_Return_Overdue_When_Target_Has_Passed()
    {
        var now = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.FromHours(10));
        var card = new CountdownCard
        {
            TargetDateTime = now.AddMinutes(-20)
        };

        var presentation = card.BuildPresentation(now);

        Assert.Equal("Overdue", presentation.StatusLabel);
        Assert.Equal("Overdue", presentation.RemainingLabel);
        Assert.Equal(0, presentation.PriorityRank);
    }

    [Fact]
    public void BuildPresentation_Should_Return_DueSoon_For_48h_And_Under()
    {
        var now = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.FromHours(10));
        var card = new CountdownCard
        {
            TargetDateTime = now.AddHours(48)
        };

        var presentation = card.BuildPresentation(now);

        Assert.Equal("Due soon", presentation.StatusLabel);
        Assert.Equal(1, presentation.PriorityRank);
    }

    [Fact]
    public void BuildPresentation_Should_Return_Upcoming_Over_48h()
    {
        var now = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.FromHours(10));
        var card = new CountdownCard
        {
            TargetDateTime = now.AddHours(50)
        };

        var presentation = card.BuildPresentation(now);

        Assert.Equal("Upcoming", presentation.StatusLabel);
        Assert.Equal(2, presentation.PriorityRank);
        Assert.Contains("remaining", presentation.RemainingLabel);
    }

    [Fact]
    public void BuildRemainingLabel_Should_Match_Presentation_RemainingLabel()
    {
        var now = new DateTimeOffset(2026, 2, 14, 12, 0, 0, TimeSpan.FromHours(10));
        var card = new CountdownCard
        {
            TargetDateTime = now.AddHours(4).AddMinutes(30)
        };

        var presentation = card.BuildPresentation(now);

        Assert.Equal(presentation.RemainingLabel, card.BuildRemainingLabel(now));
    }
}
