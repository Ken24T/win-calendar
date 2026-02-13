using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface ISettingsService
{
    Task<IReadOnlyList<AppSetting>> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task SaveSettingAsync(AppSetting setting, CancellationToken cancellationToken = default);
}
