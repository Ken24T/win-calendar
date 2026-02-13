using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(Category category, CancellationToken cancellationToken = default);
}
