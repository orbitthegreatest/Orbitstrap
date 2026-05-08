using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Dialogs;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class UninstallerDialog : WpfUiWindow, IComponentConnector
{
	public bool Confirmed { get; private set; }

	public bool KeepData { get; private set; } = true;

	public UninstallerDialog()
	{
		UninstallerDialog uninstallerDialog = this;
		UninstallerViewModel viewModel = new UninstallerViewModel();
		viewModel.ConfirmUninstallRequest += delegate
		{
			uninstallerDialog.Confirmed = true;
			uninstallerDialog.KeepData = viewModel.KeepData;
			uninstallerDialog.Close();
		};
		base.DataContext = viewModel;
		InitializeComponent();
	}
}
