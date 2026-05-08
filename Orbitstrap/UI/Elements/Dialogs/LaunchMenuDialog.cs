using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Installer;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class LaunchMenuDialog : WpfUiWindow, IComponentConnector
{
	public NextAction CloseAction;

	public LaunchMenuDialog()
	{
		LaunchMenuViewModel launchMenuViewModel = new LaunchMenuViewModel();
		launchMenuViewModel.CloseWindowRequest += delegate(object? _, NextAction closeAction)
		{
			CloseAction = closeAction;
			Close();
		};
		base.DataContext = launchMenuViewModel;
		InitializeComponent();
		if (new Random().Next(0, 10000) == 1)
		{
			LaunchTitle.Text = "Fishtrap";
		}
	}
}
