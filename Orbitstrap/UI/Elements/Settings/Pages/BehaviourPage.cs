using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class BehaviourPage : UiPage, IComponentConnector
{
	public BehaviourPage()
	{
		base.DataContext = new BehaviourViewModel();
		InitializeComponent();
	}

	private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
	{
	}
}
