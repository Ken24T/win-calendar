using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class SettingsService(ISettingsRepository settingsRepository) : ISettingsService
{
    public Task<IReadOnlyList<AppSetting>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return settingsRepository.GetAllAsync(cancellationToken);
    }

    public Task SaveSettingAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        return settingsRepository.UpsertAsync(setting, cancellationToken);
    }
}
