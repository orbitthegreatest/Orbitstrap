using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.Dialogs;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class LanguageSelectorDialog : WpfUiWindow, IComponentConnector
{
	public LanguageSelectorDialog()
	{
		LanguageSelectorViewModel languageSelectorViewModel = (LanguageSelectorViewModel)(base.DataContext = new LanguageSelectorViewModel());
		InitializeComponent();
		languageSelectorViewModel.CloseRequestEvent += delegate
		{
			Close();
		};
	}
}
