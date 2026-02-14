using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application;
using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Tests;

public class CountdownServiceTests
{
    [Fact]
    public async Task CountdownService_Should_Return_Active_Cards_Ordered_By_Sort_Then_Target()
    {
        var repository = new InMemoryCountdownRepository(
        [
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "Inactive",
                TargetDateTime = new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 1,
                IsActive = false
            },
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "B",
                TargetDateTime = new DateTimeOffset(2026, 3, 5, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 2,
                IsActive = true
            },
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "A",
                TargetDateTime = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 1,
                IsActive = true
            }
        ]);

        var services = new ServiceCollection();
        services.AddSingleton<ICountdownCardRepository>(repository);
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICountdownService>();

        var cards = await service.GetCountdownCardsAsync();

        Assert.Equal(2, cards.Count);
        Assert.Equal("A", cards[0].Title);
        Assert.Equal("B", cards[1].Title);
    }

    [Fact]
    public async Task CountdownService_Should_Return_All_Cards_For_Management()
    {
        var repository = new InMemoryCountdownRepository(
        [
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "Inactive",
                TargetDateTime = new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 1,
                IsActive = false
            },
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "B",
                TargetDateTime = new DateTimeOffset(2026, 3, 5, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 2,
                IsActive = true
            },
            new CountdownCard
            {
                Id = Guid.NewGuid(),
                Title = "A",
                TargetDateTime = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(10)),
                SortOrder = 1,
                IsActive = true
            }
        ]);

        var services = new ServiceCollection();
        services.AddSingleton<ICountdownCardRepository>(repository);
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICountdownService>();

        var cards = await service.GetCountdownCardsForManagementAsync();

        Assert.Equal(3, cards.Count);
        Assert.Equal("A", cards[0].Title);
        Assert.Equal("Inactive", cards[1].Title);
        Assert.Equal("B", cards[2].Title);
    }

    [Fact]
    public async Task CountdownService_Should_Order_Active_Card_Ties_Deterministically()
    {
        var target = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(10));

        var cardA = new CountdownCard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Title = "Same",
            TargetDateTime = target,
            SortOrder = 1,
            IsActive = true
        };

        var cardB = new CountdownCard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Title = "Same",
            TargetDateTime = target,
            SortOrder = 1,
            IsActive = true
        };

        var repository = new InMemoryCountdownRepository([cardB, cardA]);

        var services = new ServiceCollection();
        services.AddSingleton<ICountdownCardRepository>(repository);
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICountdownService>();

        var cards = await service.GetCountdownCardsAsync();

        Assert.Equal(2, cards.Count);
        Assert.Equal(cardA.Id, cards[0].Id);
        Assert.Equal(cardB.Id, cards[1].Id);
    }

    [Fact]
    public async Task CountdownService_Should_Order_Management_Ties_Deterministically()
    {
        var target = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.FromHours(10));

        var cardA = new CountdownCard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Title = "Same",
            TargetDateTime = target,
            SortOrder = 3,
            IsActive = false
        };

        var cardB = new CountdownCard
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            Title = "Same",
            TargetDateTime = target,
            SortOrder = 3,
            IsActive = true
        };

        var repository = new InMemoryCountdownRepository([cardB, cardA]);

        var services = new ServiceCollection();
        services.AddSingleton<ICountdownCardRepository>(repository);
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ICountdownService>();

        var cards = await service.GetCountdownCardsForManagementAsync();

        Assert.Equal(2, cards.Count);
        Assert.Equal(cardA.Id, cards[0].Id);
        Assert.Equal(cardB.Id, cards[1].Id);
    }

    private sealed class InMemoryCountdownRepository(IReadOnlyList<CountdownCard> items) : ICountdownCardRepository
    {
        private readonly List<CountdownCard> _items = [.. items];

        public Task<IReadOnlyList<CountdownCard>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CountdownCard>>(_items.ToList());
        }

        public Task UpsertAsync(CountdownCard countdownCard, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(item => item.Id == countdownCard.Id);
            _items.Add(countdownCard);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid countdownCardId, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(item => item.Id == countdownCardId);
            return Task.CompletedTask;
        }
    }
}
