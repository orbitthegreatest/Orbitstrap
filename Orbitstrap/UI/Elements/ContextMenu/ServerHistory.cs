using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Orbitstrap.Integrations;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.ViewModels.ContextMenu;

namespace Orbitstrap.UI.Elements.ContextMenu;

public partial class ServerHistory : WpfUiWindow, IComponentConnector
{
	public ServerHistory(ActivityWatcher watcher)
	{
		ServerHistoryViewModel serverHistoryViewModel = new ServerHistoryViewModel(watcher);
		serverHistoryViewModel.RequestCloseEvent = (EventHandler)Delegate.Combine(serverHistoryViewModel.RequestCloseEvent, (EventHandler)delegate
		{
			Close();
		});
		base.DataContext = serverHistoryViewModel;
		InitializeComponent();
	}
}
