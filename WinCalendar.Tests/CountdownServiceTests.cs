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
