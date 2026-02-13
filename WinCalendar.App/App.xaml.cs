using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WinCalendar.Application;
using WinCalendar.Infrastructure;
using WinCalendar.Infrastructure.Persistence;
using WinCalendar.Import;

namespace WinCalendar.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
	private readonly IHost _host;

	public App()
	{
		_host = Host.CreateDefaultBuilder()
			.ConfigureServices((context, services) =>
			{
				services.AddApplication();
				services.AddInfrastructure();
				services.AddRustImport();
				services.AddSingleton<MainWindow>();
				services.AddSingleton<ViewModels.ShellViewModel>();
			})
			.Build();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		await _host.StartAsync();

		var databaseMigrator = _host.Services.GetRequiredService<IDatabaseMigrator>();
		await databaseMigrator.MigrateAsync();

		var mainWindow = _host.Services.GetRequiredService<MainWindow>();
		mainWindow.DataContext = _host.Services.GetRequiredService<ViewModels.ShellViewModel>();
		mainWindow.Show();

		base.OnStartup(e);
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		await _host.StopAsync();
		_host.Dispose();
		base.OnExit(e);
	}
}

