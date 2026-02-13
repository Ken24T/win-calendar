using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IEventTemplateService
{
    Task<IReadOnlyList<EventTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    Task SaveTemplateAsync(EventTemplate template, CancellationToken cancellationToken = default);
}
