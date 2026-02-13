using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Contracts;
using WinCalendar.Application.Services;

namespace WinCalendar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEventService, EventService>();
        return services;
    }
}
