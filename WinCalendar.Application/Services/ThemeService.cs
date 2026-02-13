using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class ThemeService(
    IThemeRepository themeRepository,
    ISettingsRepository settingsRepository) : IThemeService
{
    private const string ThemeKey = "theme";

    public Task<IReadOnlyList<AppTheme>> GetThemesAsync(CancellationToken cancellationToken = default)
    {
        return themeRepository.GetAllAsync(cancellationToken);
    }

    public Task SaveThemeAsync(AppTheme theme, CancellationToken cancellationToken = default)
    {
        return themeRepository.UpsertAsync(theme, cancellationToken);
    }

    public async Task<string?> GetActiveThemeNameAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsRepository.GetAllAsync(cancellationToken);
        return settings.FirstOrDefault(x => string.Equals(x.Key, ThemeKey, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    public Task SetActiveThemeNameAsync(string themeName, CancellationToken cancellationToken = default)
    {
        return settingsRepository.UpsertAsync(new AppSetting
        {
            Key = ThemeKey,
            Value = themeName
        }, cancellationToken);
    }
}
