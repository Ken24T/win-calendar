using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Contracts;
using WinCalendar.Application.Services;

namespace WinCalendar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();
        services.AddSingleton<IEventSearchService, EventSearchService>();
        services.AddSingleton<IRecurrenceService, RecurrenceService>();
        services.AddSingleton<ICategoryService, CategoryService>();
        services.AddSingleton<IEventTemplateService, EventTemplateService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        return services;
    }
}
