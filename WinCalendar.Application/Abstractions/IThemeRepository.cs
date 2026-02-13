using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface IThemeRepository
{
    Task<IReadOnlyList<AppTheme>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(AppTheme theme, CancellationToken cancellationToken = default);
}
