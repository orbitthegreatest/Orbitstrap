using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.Elements.ContextMenu;

public partial class ServerInformation : WpfUiWindow, IComponentConnector
{
	public ServerInformation(Watcher watcher)
	{
		base.DataContext = new ServerInformationViewModel(watcher);
		InitializeComponent();
	}
}
