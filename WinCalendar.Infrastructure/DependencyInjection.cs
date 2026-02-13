using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Abstractions;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Infrastructure.Repositories;

namespace WinCalendar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<SqliteConnectionFactory>();
        services.AddSingleton<IDatabaseMigrator, SqliteDatabaseMigrator>();
        services.AddSingleton<IEventRepository, SqliteEventRepository>();
        services.AddSingleton<ICategoryRepository, SqliteCategoryRepository>();
        services.AddSingleton<IEventTemplateRepository, SqliteEventTemplateRepository>();
        services.AddSingleton<ISettingsRepository, SqliteSettingsRepository>();
        return services;
    }
}
