using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class AddFastFlagDialog : WpfUiWindow, IComponentConnector
{
	public MessageBoxResult Result = MessageBoxResult.Cancel;

	public AddFastFlagDialog()
	{
		InitializeComponent();
	}

	private void ImportButton_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = Strings.FileTypes_JSONFiles + "|*.json"
		};
		if (openFileDialog.ShowDialog() == true)
		{
			JsonTextBox.Text = File.ReadAllText(openFileDialog.FileName);
		}
	}

	private void OKButton_Click(object sender, RoutedEventArgs e)
	{
		Result = MessageBoxResult.OK;
		Close();
	}
}
