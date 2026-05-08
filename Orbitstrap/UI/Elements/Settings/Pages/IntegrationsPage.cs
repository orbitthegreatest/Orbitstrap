using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Orbitstrap.Models;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class IntegrationsPage : UiPage, IComponentConnector
{
	public IntegrationsPage()
	{
		base.DataContext = new IntegrationsViewModel();
		InitializeComponent();
	}

	public void CustomIntegrationSelection(object sender, SelectionChangedEventArgs e)
	{
		IntegrationsViewModel obj = (IntegrationsViewModel)base.DataContext;
		obj.SelectedCustomIntegration = (CustomIntegration)((ListBox)sender).SelectedItem;
		obj.OnPropertyChanged("SelectedCustomIntegration");
	}
}
