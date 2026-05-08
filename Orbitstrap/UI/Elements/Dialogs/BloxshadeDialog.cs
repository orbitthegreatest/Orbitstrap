using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using Orbitstrap.Enums;
using Orbitstrap.UI.Elements.Base;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class BloxshadeDialog : WpfUiWindow, IComponentConnector
{
	public NextAction CloseAction;

	public BloxshadeDialog()
	{
		InitializeComponent();
	}

	public void Close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}
}
