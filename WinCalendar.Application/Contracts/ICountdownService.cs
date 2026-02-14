using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface ICountdownService
{
    Task<IReadOnlyList<CountdownCard>> GetCountdownCardsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CountdownCard>> GetCountdownCardsForManagementAsync(CancellationToken cancellationToken = default);

    Task SaveCountdownCardAsync(CountdownCard countdownCard, CancellationToken cancellationToken = default);

    Task DeleteCountdownCardAsync(Guid countdownCardId, CancellationToken cancellationToken = default);
}
