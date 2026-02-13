using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface IThemeService
{
    Task<IReadOnlyList<AppTheme>> GetThemesAsync(CancellationToken cancellationToken = default);

    Task SaveThemeAsync(AppTheme theme, CancellationToken cancellationToken = default);

    Task<string?> GetActiveThemeNameAsync(CancellationToken cancellationToken = default);

    Task SetActiveThemeNameAsync(string themeName, CancellationToken cancellationToken = default);
}
