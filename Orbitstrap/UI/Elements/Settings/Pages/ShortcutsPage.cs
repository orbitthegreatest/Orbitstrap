using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class ShortcutsPage : UiPage, IComponentConnector
{
	public ShortcutsPage()
	{
		base.DataContext = new ShortcutsViewModel();
		InitializeComponent();
	}
}
