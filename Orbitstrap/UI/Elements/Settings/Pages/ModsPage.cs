using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.UI.ViewModels.Settings;
using Wpf.Ui.Controls;

namespace Orbitstrap.UI.Elements.Settings.Pages;

public partial class ModsPage : UiPage, IComponentConnector
{
	public ModsPage()
	{
		base.DataContext = new ModsViewModel();
		InitializeComponent();
		// Populate the skybox dropdown from the bundled Skyboxes\ folder
		Loaded += async (_, _) => await ((ModsViewModel)DataContext).LoadSkyboxPacksAsync();
	}
}
