using System.Windows;
using System.Diagnostics;
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
		var diagnosticsVerbose = Debugger.IsAttached ||
			string.Equals(Environment.GetEnvironmentVariable("WINCALENDAR_DIAGNOSTICS_VERBOSE"), "1", StringComparison.Ordinal);
		StartupDiagnostics.Initialise(enableInfoLogging: diagnosticsVerbose);

		RegisterGlobalExceptionHandlers();
		StartupDiagnostics.WriteInfo("App constructor started.");

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

		StartupDiagnostics.WriteInfo("Host built successfully.");
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		try
		{
			StartupDiagnostics.WriteInfo("OnStartup begin.");
			await _host.StartAsync();
			StartupDiagnostics.WriteInfo("Host started.");

			var databaseMigrator = _host.Services.GetRequiredService<IDatabaseMigrator>();
			await databaseMigrator.MigrateAsync();
			StartupDiagnostics.WriteInfo("Database migration completed.");

			var mainWindow = _host.Services.GetRequiredService<MainWindow>();
			mainWindow.DataContext = _host.Services.GetRequiredService<ViewModels.ShellViewModel>();
			mainWindow.Show();
			StartupDiagnostics.WriteInfo("Main window shown.");

			base.OnStartup(e);
		}
		catch (Exception exception)
		{
			StartupDiagnostics.WriteError("Fatal startup exception.", exception);
			MessageBox.Show(
				$"WinCalendar failed to start.\n\n{exception.Message}\n\nDiagnostics log:\n{StartupDiagnostics.LogPath}",
				"WinCalendar Startup Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error);

			Shutdown(-1);
		}
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		try
		{
			StartupDiagnostics.WriteInfo("OnExit begin.");
			await _host.StopAsync();
			_host.Dispose();
			StartupDiagnostics.WriteInfo("Host stopped and disposed.");
		}
		catch (Exception exception)
		{
			StartupDiagnostics.WriteError("Exception during OnExit.", exception);
		}

		base.OnExit(e);
	}

	private void RegisterGlobalExceptionHandlers()
	{
		DispatcherUnhandledException += (_, args) =>
		{
			if (IsNonCriticalException(args.Exception))
			{
				StartupDiagnostics.WriteInfo($"Ignored non-critical dispatcher exception: {args.Exception.Message}");
				args.Handled = true;
				return;
			}

			StartupDiagnostics.WriteError("Dispatcher unhandled exception.", args.Exception);
			MessageBox.Show(
				$"Unexpected UI error:\n\n{args.Exception.Message}\n\nDiagnostics log:\n{StartupDiagnostics.LogPath}",
				"WinCalendar Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			args.Handled = true;
		};

		AppDomain.CurrentDomain.UnhandledException += (_, args) =>
		{
			if (args.ExceptionObject is Exception exception)
			{
				StartupDiagnostics.WriteError("AppDomain unhandled exception.", exception);
			}
			else
			{
				StartupDiagnostics.WriteInfo($"AppDomain unhandled non-exception object: {args.ExceptionObject}");
			}
		};

		TaskScheduler.UnobservedTaskException += (_, args) =>
		{
			if (IsNonCriticalException(args.Exception))
			{
				StartupDiagnostics.WriteInfo($"Ignored non-critical task exception: {args.Exception.Message}");
				args.SetObserved();
				return;
			}

			StartupDiagnostics.WriteError("Unobserved task exception.", args.Exception);
			args.SetObserved();
		};
	}

	private static bool IsNonCriticalException(Exception exception)
	{
		return exception is OperationCanceledException or TaskCanceledException;
	}
}

