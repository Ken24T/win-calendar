using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class EventTemplateService(IEventTemplateRepository templateRepository) : IEventTemplateService
{
    public Task<IReadOnlyList<EventTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return templateRepository.GetAllAsync(cancellationToken);
    }

    public Task SaveTemplateAsync(EventTemplate template, CancellationToken cancellationToken = default)
    {
        return templateRepository.UpsertAsync(template, cancellationToken);
    }
}
