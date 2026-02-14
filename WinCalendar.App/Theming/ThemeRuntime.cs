using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace WinCalendar.App.Theming;

internal static class ThemeRuntime
{
    public static void Apply(AppThemeMode mode)
    {
        var effectiveMode = mode == AppThemeMode.System
            ? GetSystemMode()
            : mode;

        var app = System.Windows.Application.Current;
        if (app is null)
        {
            return;
        }

        if (effectiveMode == AppThemeMode.Dark)
        {
            SetBrush(app, "AppWindowBackgroundBrush", "#1E1E1E");
            SetBrush(app, "AppPanelBackgroundBrush", "#2A2A2A");
            SetBrush(app, "AppPanelMutedBackgroundBrush", "#33373F");
            SetBrush(app, "AppOverlayBackgroundBrush", "#CC2A2A2A");
            SetBrush(app, "AppPrimaryTextBrush", "#F1F1F1");
            SetBrush(app, "AppSecondaryTextBrush", "#B9C0CC");
            SetBrush(app, "AppBorderBrush", "#3F4653");
            SetBrush(app, "AppSubtleBorderBrush", "#3A414D");
            SetBrush(app, "AppErrorTextBrush", "#FF8A8A");
            SetBrush(app, "AppInputBackgroundBrush", "#2F343D");
            SetBrush(app, "AppInputForegroundBrush", "#F1F1F1");
            SetBrush(app, "AppInputBorderBrush", "#434B58");
            SetBrush(app, "AppInputDisabledBackgroundBrush", "#2F343D");
            SetBrush(app, "AppInputDisabledForegroundBrush", "#8A93A3");
            SetBrush(app, "AppSelectionBackgroundBrush", "#3A4350");
            SetBrush(app, "AppSelectionForegroundBrush", "#F1F1F1");
            SetBrush(app, "AppAccentBrush", "#6AA8FF");
            SetBrush(app, "AppDayEventHeaderBrush", "#2F4F7C");
            SetBrush(app, "AppDayEventBodyBrush", "#1F2A3E");
            SetBrush(app, "AppDayEventHeaderTextBrush", "#F5F8FF");
            SetBrush(app, "AppDayEventBodyTextBrush", "#D6E1F5");
            SetBrush(app, "AppDayEventSeparatorBrush", "#36455F");
            SetBrush(app, "AppMenuBackgroundBrush", "#2A2A2A");
            SetBrush(app, "AppMenuForegroundBrush", "#F1F1F1");
            SetBrush(app, "AppMenuHoverBackgroundBrush", "#3A4350");
        }
        else
        {
            SetBrush(app, "AppWindowBackgroundBrush", "#F7F7F9");
            SetBrush(app, "AppPanelBackgroundBrush", "#FFFFFF");
            SetBrush(app, "AppPanelMutedBackgroundBrush", "#F0F3F8");
            SetBrush(app, "AppOverlayBackgroundBrush", "#CCFFFFFF");
            SetBrush(app, "AppPrimaryTextBrush", "#24272E");
            SetBrush(app, "AppSecondaryTextBrush", "#626B7A");
            SetBrush(app, "AppBorderBrush", "#D9DEE8");
            SetBrush(app, "AppSubtleBorderBrush", "#E5EAF3");
            SetBrush(app, "AppErrorTextBrush", "#B42318");
            SetBrush(app, "AppInputBackgroundBrush", "#FFFFFF");
            SetBrush(app, "AppInputForegroundBrush", "#24272E");
            SetBrush(app, "AppInputBorderBrush", "#C5CCDA");
            SetBrush(app, "AppInputDisabledBackgroundBrush", "#F1F3F7");
            SetBrush(app, "AppInputDisabledForegroundBrush", "#8A93A3");
            SetBrush(app, "AppSelectionBackgroundBrush", "#DCE8FF");
            SetBrush(app, "AppSelectionForegroundBrush", "#1E2A44");
            SetBrush(app, "AppAccentBrush", "#0A64D6");
            SetBrush(app, "AppDayEventHeaderBrush", "#7B95BE");
            SetBrush(app, "AppDayEventBodyBrush", "#C9D4E8");
            SetBrush(app, "AppDayEventHeaderTextBrush", "#FFFFFF");
            SetBrush(app, "AppDayEventBodyTextBrush", "#20304A");
            SetBrush(app, "AppDayEventSeparatorBrush", "#E7EDF8");
            SetBrush(app, "AppMenuBackgroundBrush", "#FFFFFF");
            SetBrush(app, "AppMenuForegroundBrush", "#24272E");
            SetBrush(app, "AppMenuHoverBackgroundBrush", "#E9EEF8");
        }
    }

    public static AppThemeMode Parse(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "light" => AppThemeMode.Light,
            "dark" => AppThemeMode.Dark,
            _ => AppThemeMode.System
        };
    }

    public static string ToPersistedValue(AppThemeMode mode)
    {
        return mode switch
        {
            AppThemeMode.Light => "light",
            AppThemeMode.Dark => "dark",
            _ => "system"
        };
    }

    private static AppThemeMode GetSystemMode()
    {
        try
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue == 0 ? AppThemeMode.Dark : AppThemeMode.Light;
            }
        }
        catch
        {
            // Fall back to light mode when system mode cannot be detected.
        }

        return AppThemeMode.Light;
    }

    private static void SetBrush(System.Windows.Application app, string key, string hex)
    {
        var brush = (Brush)new BrushConverter().ConvertFromString(hex)!;
        brush.Freeze();
        app.Resources[key] = brush;
    }
}
