using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Contracts;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task SaveCategoryAsync(Category category, CancellationToken cancellationToken = default);
}
