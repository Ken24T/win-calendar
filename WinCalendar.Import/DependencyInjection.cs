using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Import.Contracts;
using WinCalendar.Import.Services;

namespace WinCalendar.Import;

public static class DependencyInjection
{
    public static IServiceCollection AddRustImport(this IServiceCollection services)
    {
        services.AddSingleton<RustSourceReader>();
        services.AddSingleton<TargetDataWriter>();
        services.AddSingleton<IRustCalendarImporter, RustCalendarImporter>();
        return services;
    }
}
