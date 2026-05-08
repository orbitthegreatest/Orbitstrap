using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Shell;
using Orbitstrap.Extensions;
using Orbitstrap.UI.Utility;

namespace Orbitstrap.UI.Elements.Bootstrapper.Base;

public class WinFormsDialogBase : Form, IBootstrapperDialog
{
	public const int TaskbarProgressMaximum = 100;

	private bool _isClosing;

	public Orbitstrap.Bootstrapper? Bootstrapper { get; set; }

	protected virtual string _message { get; set; } = "Please wait...";

	protected virtual ProgressBarStyle _progressStyle { get; set; }

	protected virtual int _progressValue { get; set; }

	protected virtual int _progressMaximum { get; set; }

	protected virtual TaskbarItemProgressState _taskbarProgressState { get; set; }

	protected virtual double _taskbarProgressValue { get; set; }

	protected virtual bool _cancelEnabled { get; set; }

	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(() => _message = value);
			}
			else
			{
				_message = value;
			}
		}
	}

	public ProgressBarStyle ProgressStyle
	{
		get
		{
			return _progressStyle;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(() => _progressStyle = value);
			}
			else
			{
				_progressStyle = value;
			}
		}
	}

	public int ProgressMaximum
	{
		get
		{
			return _progressMaximum;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(() => _progressMaximum = value);
			}
			else
			{
				_progressMaximum = value;
			}
		}
	}

	public int ProgressValue
	{
		get
		{
			return _progressValue;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(() => _progressValue = value);
			}
			else
			{
				_progressValue = value;
			}
		}
	}

	public TaskbarItemProgressState TaskbarProgressState
	{
		get
		{
			return _taskbarProgressState;
		}
		set
		{
			_taskbarProgressState = value;
			TaskbarProgress.SetProgressState(Process.GetCurrentProcess().MainWindowHandle, value);
		}
	}

	public double TaskbarProgressValue
	{
		get
		{
			return _taskbarProgressValue;
		}
		set
		{
			_taskbarProgressValue = value;
			TaskbarProgress.SetProgressValue(Process.GetCurrentProcess().MainWindowHandle, (int)value, 100);
		}
	}

	public bool CancelEnabled
	{
		get
		{
			return _cancelEnabled;
		}
		set
		{
			if (base.InvokeRequired)
			{
				Invoke(() => _cancelEnabled = value);
			}
			else
			{
				_cancelEnabled = value;
			}
		}
	}

	public void ScaleWindow()
	{
		Size size = (MaximumSize = WindowScaling.GetScaledSize(base.Size));
		Size size2 = (MinimumSize = size);
		base.Size = size2;
		foreach (Control control in base.Controls)
		{
			control.Size = WindowScaling.GetScaledSize(control.Size);
			control.Location = WindowScaling.GetScaledPoint(control.Location);
			control.Padding = WindowScaling.GetScaledPadding(control.Padding);
		}
	}

	public void SetupDialog()
	{
		Text = App.Settings.Prop.BootstrapperTitle;
		base.Icon = App.Settings.Prop.BootstrapperIcon.GetIcon();
		if (Locale.RightToLeft)
		{
			RightToLeft = RightToLeft.Yes;
			RightToLeftLayout = true;
		}
	}

	public void ButtonCancel_Click(object? sender, EventArgs e)
	{
		Close();
	}

	public void Dialog_FormClosing(object sender, FormClosingEventArgs e)
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

	public virtual void CloseBootstrapper()
	{
		if (base.InvokeRequired)
		{
			Invoke(CloseBootstrapper);
			return;
		}
		_isClosing = true;
		Close();
	}

	public virtual void ShowSuccess(string message, Action? callback)
	{
		BaseFunctions.ShowSuccess(message, callback);
	}
}
