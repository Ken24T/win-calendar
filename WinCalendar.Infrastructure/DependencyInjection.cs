using Microsoft.Extensions.DependencyInjection;
using WinCalendar.Application.Abstractions;
using WinCalendar.Infrastructure.Repositories;

namespace WinCalendar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        return services;
    }
}
