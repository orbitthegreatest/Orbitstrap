using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.UI.Elements.Bootstrapper.Base;
using Orbitstrap.UI.ViewModels.Bootstrapper;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public partial class ByfronDialog : Window, IBootstrapperDialog, IComponentConnector
{
	private readonly ByfronDialogViewModel _viewModel;

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
			string text = value;
			if (text.EndsWith("..."))
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 3);
			}
			_viewModel.Message = text;
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
			_viewModel.OnPropertyChanged("CancelEnabled");
			_viewModel.OnPropertyChanged("CancelButtonVisibility");
			_viewModel.OnPropertyChanged("VersionTextVisibility");
			_viewModel.OnPropertyChanged("VersionText");
		}
	}

	public ByfronDialog()
	{
		_viewModel = new ByfronDialogViewModel(this, Utilities.GetRobloxVersionStr(Bootstrapper?.IsStudioLaunch ?? false));
		base.DataContext = _viewModel;
		base.Title = App.Settings.Prop.BootstrapperTitle;
		base.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon().GetImageSource();
		if (App.Settings.Prop.Theme.GetFinal() == Theme.Light)
		{
			_viewModel.DialogBorder = new Thickness(1.0);
			_viewModel.Background = new SolidColorBrush(Color.FromRgb(242, 244, 245));
			_viewModel.Foreground = new SolidColorBrush(Color.FromRgb(57, 59, 61));
			_viewModel.IconColor = new SolidColorBrush(Color.FromRgb(57, 59, 61));
			_viewModel.ProgressBarBackground = new SolidColorBrush(Color.FromRgb(189, 190, 190));
			_viewModel.ByfronLogoLocation = new BitmapImage(new Uri("pack://application:,,,/Resources/BootstrapperStyles/ByfronDialog/ByfronLogoLight.jpg"));
		}
		InitializeComponent();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
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
