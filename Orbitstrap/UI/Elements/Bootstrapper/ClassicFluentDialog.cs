using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Shell;
using Orbitstrap.Extensions;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.Elements.Bootstrapper.Base;
using Orbitstrap.UI.ViewModels.Bootstrapper;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public partial class ClassicFluentDialog : WpfUiWindow, IBootstrapperDialog, IComponentConnector
{
	private readonly BootstrapperDialogViewModel _viewModel;

	private bool _isClosing;

	public Orbitstrap.Bootstrapper? Bootstrapper { get; set; }

	public string Message
	{
		get
		{
			return _viewModel.Message;
		}
		set
		{
			_viewModel.Message = value;
			_viewModel.OnPropertyChanged("Message");
		}
	}

	public ProgressBarStyle ProgressStyle
	{
		get
		{
			if (!_viewModel.ProgressIndeterminate)
			{
				return ProgressBarStyle.Continuous;
			}
			return ProgressBarStyle.Marquee;
		}
		set
		{
			_viewModel.ProgressIndeterminate = value == ProgressBarStyle.Marquee;
			_viewModel.OnPropertyChanged("ProgressIndeterminate");
		}
	}

	public int ProgressMaximum
	{
		get
		{
			return _viewModel.ProgressMaximum;
		}
		set
		{
			_viewModel.ProgressMaximum = value;
			_viewModel.OnPropertyChanged("ProgressMaximum");
		}
	}

	public int ProgressValue
	{
		get
		{
			return _viewModel.ProgressValue;
		}
		set
		{
			_viewModel.ProgressValue = value;
			_viewModel.OnPropertyChanged("ProgressValue");
		}
	}

	public TaskbarItemProgressState TaskbarProgressState
	{
		get
		{
			return _viewModel.TaskbarProgressState;
		}
		set
		{
			_viewModel.TaskbarProgressState = value;
			_viewModel.OnPropertyChanged("TaskbarProgressState");
		}
	}

	public double TaskbarProgressValue
	{
		get
		{
			return _viewModel.TaskbarProgressValue;
		}
		set
		{
			_viewModel.TaskbarProgressValue = value;
			_viewModel.OnPropertyChanged("TaskbarProgressValue");
		}
	}

	public bool CancelEnabled
	{
		get
		{
			return _viewModel.CancelEnabled;
		}
		set
		{
			_viewModel.CancelEnabled = value;
			_viewModel.OnPropertyChanged("CancelButtonVisibility");
			_viewModel.OnPropertyChanged("CancelEnabled");
		}
	}

	public ClassicFluentDialog()
	{
		InitializeComponent();
		_viewModel = new ClassicFluentDialogViewModel(this);
		base.DataContext = _viewModel;
		base.Title = App.Settings.Prop.BootstrapperTitle;
		base.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();
	}

	private void UiWindow_Closing(object sender, CancelEventArgs e)
	{
		if (!_isClosing)
		{
			Bootstrapper?.Cancel();
		}
	}

	public void ShowBootstrapper()
	{
		ShowDialog();
	}

	public void CloseBootstrapper()
	{
		_isClosing = true;
		base.Dispatcher.BeginInvoke(new Action(base.Close));
	}

	public void ShowSuccess(string message, Action? callback)
	{
		BaseFunctions.ShowSuccess(message, callback);
	}
}
