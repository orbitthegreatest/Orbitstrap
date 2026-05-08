using System;
using System.Windows;

namespace Orbitstrap.UI.Elements.Bootstrapper.Base;

internal static class BaseFunctions
{
	public static void ShowSuccess(string message, Action? callback)
	{
		Frontend.ShowMessageBox(message, MessageBoxImage.Asterisk);
		callback?.Invoke();
		App.Terminate();
	}
}
