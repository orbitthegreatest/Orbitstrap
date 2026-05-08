using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Orbitstrap.Enums;
using Orbitstrap.Properties;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Bootstrapper;
using Orbitstrap.UI.Elements.Dialogs;

namespace Orbitstrap.UI;

internal static class Frontend
{
	public static MessageBoxResult ShowMessageBox(string message, MessageBoxImage icon = MessageBoxImage.None, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxResult defaultResult = MessageBoxResult.None)
	{
		App.Logger.WriteLine("Frontend::ShowMessageBox", message);
		if ((App.LaunchSettings?.QuietFlag.Active == true))
		{
			return defaultResult;
		}
		return ShowFluentMessageBox(message, icon, buttons);
	}

	public static void ShowPlayerErrorDialog(bool crash = false)
	{
		if (!(App.LaunchSettings?.QuietFlag.Active == true))
		{
			string text = Strings.Dialog_PlayerError_FailedLaunch;
			if (crash)
			{
				text = Strings.Dialog_PlayerError_Crash;
			}
			string text2 = string.Format(Strings.Dialog_PlayerError_HelpInformation, "https://github.com/orbitthegreatest/Orbitstrap/wiki/Roblox-crashes-or-does-not-launch", "https://github.com/orbitthegreatest/Orbitstrap/wiki/Switching-between-Roblox-and-Orbitstrap");
			ShowMessageBox(text + "\n\n" + text2, MessageBoxImage.Hand);
		}
	}

	public static void ShowExceptionDialog(Exception exception)
	{
		if (!(App.LaunchSettings?.QuietFlag.Active == true))
		{
			System.Windows.Application.Current.Dispatcher.Invoke(delegate
			{
				new ExceptionDialog(exception).ShowDialog();
			});
		}
	}

	public static void ShowConnectivityDialog(string title, string description, MessageBoxImage image, Exception exception)
	{
		if (!(App.LaunchSettings?.QuietFlag.Active == true))
		{
			System.Windows.Application.Current.Dispatcher.Invoke(delegate
			{
				new ConnectivityDialog(title, description, image, exception).ShowDialog();
			});
		}
	}

	private static IBootstrapperDialog GetCustomBootstrapper()
	{
		Directory.CreateDirectory(Paths.CustomThemes);
		try
		{
			if (App.Settings.Prop.SelectedCustomTheme == null)
			{
				throw new Exception("No custom theme selected");
			}
			CustomDialog customDialog = new CustomDialog();
			customDialog.ApplyCustomTheme(App.Settings.Prop.SelectedCustomTheme);
			return customDialog;
		}
		catch (Exception ex)
		{
			App.Logger.WriteException("Frontend::GetCustomBootstrapper", ex);
			if (!(App.LaunchSettings?.QuietFlag.Active == true))
			{
				ShowMessageBox("Failed to setup custom bootstrapper: " + ex.Message + ".\nDefaulting to Fluent.", MessageBoxImage.Hand);
			}
			return GetBootstrapperDialog(BootstrapperStyle.FluentDialog);
		}
	}

	public static IBootstrapperDialog GetBootstrapperDialog(BootstrapperStyle style)
	{
		return style switch
		{
			BootstrapperStyle.VistaDialog => new VistaDialog(), 
			BootstrapperStyle.LegacyDialog2008 => new LegacyDialog2008(), 
			BootstrapperStyle.LegacyDialog2011 => new LegacyDialog2011(), 
			BootstrapperStyle.ProgressDialog => new ProgressDialog(), 
			BootstrapperStyle.ClassicFluentDialog => new ClassicFluentDialog(), 
			BootstrapperStyle.ByfronDialog => new ByfronDialog(), 
			BootstrapperStyle.FluentDialog => new FluentDialog(aero: false), 
			BootstrapperStyle.FluentAeroDialog => new FluentDialog(aero: true), 
			BootstrapperStyle.CustomDialog => GetCustomBootstrapper(), 
			_ => new FluentDialog(aero: false), 
		};
	}

	private static MessageBoxResult ShowFluentMessageBox(string message, MessageBoxImage icon, MessageBoxButton buttons)
	{
		return System.Windows.Application.Current.Dispatcher.Invoke(delegate
		{
			FluentMessageBox fluentMessageBox = new FluentMessageBox(message, icon, buttons);
			fluentMessageBox.ShowDialog();
			return fluentMessageBox.Result;
		});
	}

	public static void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.None, int timeout = 5)
	{
		NotifyIcon notifyIcon = new NotifyIcon();
		notifyIcon.Icon = Orbitstrap.Properties.Resources.IconOrbitstrap;
		notifyIcon.Text = "Orbitstrap";
		notifyIcon.Visible = true;
		notifyIcon.ShowBalloonTip(timeout, title, message, icon);
	}
}
