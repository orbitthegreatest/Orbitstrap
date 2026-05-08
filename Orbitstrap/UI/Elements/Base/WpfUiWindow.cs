using System;
using System.Windows;
using System.Windows.Interop;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Orbitstrap.UI.Elements.Base;

public abstract class WpfUiWindow : UiWindow
{
    private static readonly IThemeService _themeService = new ThemeService();

    public bool DefaultBorderEnabled { get; set; } = true;
    public Wpf.Ui.Appearance.ThemeType DefaultBorderThemeOverwrite { get; set; } = Wpf.Ui.Appearance.ThemeType.Unknown;

    public WpfUiWindow()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        try
        {
            var finalTheme = App.Settings?.Prop?.Theme.GetFinal() ?? Orbitstrap.Enums.Theme.Dark;
            _themeService.SetTheme(finalTheme == Orbitstrap.Enums.Theme.Dark ? ThemeType.Dark : ThemeType.Light);
            _themeService.SetSystemAccent();

            string themeName = Enum.GetName(typeof(Orbitstrap.Enums.Theme), finalTheme) ?? "Dark";

            // Load the theme ResourceDictionary. Try multiple URI formats.
            ResourceDictionary? dict = TryLoadThemeDict(themeName);
            if (dict != null && Application.Current?.Resources?.MergedDictionaries?.Count > 2)
            {
                Application.Current.Resources.MergedDictionaries[2] = dict;
            }
        }
        catch
        {
            // Swallow all theme errors - the app already loaded themes via App.xaml
        }
    }

    private static ResourceDictionary? TryLoadThemeDict(string themeName)
    {
        // Try 1: relative URI (no leading slash)
        try
        {
            return new ResourceDictionary
            {
                Source = new Uri($"UI/Style/{themeName}.xaml", UriKind.Relative)
            };
        }
        catch { }

        // Try 2: absolute pack URI
        try
        {
            return new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/UI/Style/{themeName}.xaml")
            };
        }
        catch { }

        // Try 3: component URI (searches all loaded assemblies)
        try
        {
            return new ResourceDictionary
            {
                Source = new Uri($"/Orbitstrap;component/UI/Style/{themeName}.xaml", UriKind.Relative)
            };
        }
        catch { }

        return null;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        if ((App.Settings?.Prop?.WPFSoftwareRender == true || App.LaunchSettings?.NoGPUFlag.Active == true)
            && PresentationSource.FromVisual(this) is HwndSource hwndSource)
        {
            hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        }
        base.OnSourceInitialized(e);
    }
}
