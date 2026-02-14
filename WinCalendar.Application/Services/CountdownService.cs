using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class CountdownService(ICountdownCardRepository countdownCardRepository) : ICountdownService
{
    public async Task<IReadOnlyList<CountdownCard>> GetCountdownCardsAsync(CancellationToken cancellationToken = default)
    {
        var items = await countdownCardRepository.GetAllAsync(cancellationToken);

        return items
            .Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.TargetDateTime)
            .ToList();
    }

    public async Task<IReadOnlyList<CountdownCard>> GetCountdownCardsForManagementAsync(CancellationToken cancellationToken = default)
    {
        var items = await countdownCardRepository.GetAllAsync(cancellationToken);

        return items
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.TargetDateTime)
            .ThenBy(item => item.Title)
            .ToList();
    }

    public Task SaveCountdownCardAsync(CountdownCard countdownCard, CancellationToken cancellationToken = default)
    {
        return countdownCardRepository.UpsertAsync(countdownCard, cancellationToken);
    }

    public Task DeleteCountdownCardAsync(Guid countdownCardId, CancellationToken cancellationToken = default)
    {
        return countdownCardRepository.DeleteAsync(countdownCardId, cancellationToken);
    }
}
