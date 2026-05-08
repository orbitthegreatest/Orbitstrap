using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class AppearancePage : UiPage, IComponentConnector
{
	public AppearancePage()
	{
		base.DataContext = new AppearanceViewModel(this);
		InitializeComponent();
	}

	public void CustomThemeSelection(object sender, SelectionChangedEventArgs e)
	{
		AppearanceViewModel obj = (AppearanceViewModel)base.DataContext;
		obj.SelectedCustomTheme = (string)((ListBox)sender).SelectedItem;
		obj.SelectedCustomThemeName = obj.SelectedCustomTheme;
		obj.OnPropertyChanged("SelectedCustomTheme");
		obj.OnPropertyChanged("SelectedCustomThemeName");
	}
}
