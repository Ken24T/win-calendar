using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface ISettingsRepository
{
    Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(AppSetting setting, CancellationToken cancellationToken = default);
}
