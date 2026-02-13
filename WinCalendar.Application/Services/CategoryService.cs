using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Domain.Entities;

namespace WinCalendar.Application.Services;

internal sealed class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return categoryRepository.GetAllAsync(cancellationToken);
    }

    public Task SaveCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        return categoryRepository.UpsertAsync(category, cancellationToken);
    }
}
