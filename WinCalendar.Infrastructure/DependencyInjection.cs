using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Abstractions;
using WinCalendar.Application.Contracts;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Infrastructure.Repositories;
using WinCalendar.Infrastructure.Services;

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
        services.AddSingleton<IThemeRepository, SqliteThemeRepository>();
        services.AddSingleton<IBackupService, DatabaseBackupService>();
        return services;
    }
}
