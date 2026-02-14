using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface ICountdownCardRepository
{
    Task<IReadOnlyList<CountdownCard>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(CountdownCard countdownCard, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid countdownCardId, CancellationToken cancellationToken = default);
}
