using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface IEventTemplateRepository
{
    Task<IReadOnlyList<EventTemplate>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(EventTemplate template, CancellationToken cancellationToken = default);
}
