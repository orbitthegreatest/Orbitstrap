using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orbitstrap.Extensions;
using Orbitstrap.UI.Elements.Bootstrapper.Base;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public class VistaDialog : WinFormsDialogBase
{
	private TaskDialogPage _dialogPage;

	private IContainer components = null!;

	protected sealed override string _message
	{
		get
		{
			return _dialogPage.Heading ?? "";
		}
		set
		{
			_dialogPage.Heading = value;
		}
	}

	protected sealed override ProgressBarStyle _progressStyle
	{
		get
		{
			return base._progressStyle;
		}
		set
		{
			if (_dialogPage.ProgressBar != null)
			{
				TaskDialogProgressBar progressBar = _dialogPage.ProgressBar;
				progressBar.State = value switch
				{
					ProgressBarStyle.Continuous => TaskDialogProgressBarState.Normal, 
					ProgressBarStyle.Blocks => TaskDialogProgressBarState.Normal, 
					ProgressBarStyle.Marquee => TaskDialogProgressBarState.Marquee, 
					_ => _dialogPage.ProgressBar.State, 
				};
			}
		}
	}

	protected sealed override int _progressMaximum
	{
		get
		{
			return _dialogPage.ProgressBar?.Maximum ?? 0;
		}
		set
		{
			if (_dialogPage.ProgressBar != null)
			{
				_dialogPage.ProgressBar.Maximum = value;
			}
		}
	}

	protected sealed override int _progressValue
	{
		get
		{
			return _dialogPage.ProgressBar?.Value ?? 0;
		}
		set
		{
			if (_dialogPage.ProgressBar != null)
			{
				_dialogPage.ProgressBar.Value = value;
			}
		}
	}

	protected sealed override bool _cancelEnabled
	{
		get
		{
			return _dialogPage.Buttons[0].Enabled;
		}
		set
		{
			_dialogPage.Buttons[0].Enabled = value;
		}
	}

	public VistaDialog()
	{
		InitializeComponent();
		_dialogPage = new TaskDialogPage
		{
			Icon = new TaskDialogIcon(App.Settings.Prop.BootstrapperIcon.GetIcon()),
			Caption = App.Settings.Prop.BootstrapperTitle,
			RightToLeftLayout = Locale.RightToLeft,
			Buttons = { TaskDialogButton.Cancel },
			ProgressBar = new TaskDialogProgressBar
			{
				State = TaskDialogProgressBarState.Marquee
			}
		};
		_message = "Please wait...";
		_cancelEnabled = false;
		_dialogPage.Buttons[0].Click += base.ButtonCancel_Click;
		SetupDialog();
	}

	public override void ShowSuccess(string message, Action? callback)
	{
		if (base.InvokeRequired)
		{
			Invoke(new Action<string, Action>(ShowSuccess), message, callback);
			return;
		}
		TaskDialogPage taskDialogPage = new TaskDialogPage
		{
			Icon = TaskDialogIcon.ShieldSuccessGreenBar,
			Caption = App.Settings.Prop.BootstrapperTitle,
			Heading = message,
			Buttons = { TaskDialogButton.OK }
		};
		taskDialogPage.Buttons[0].Click += delegate
		{
			if (callback != null)
			{
				callback();
			}
			App.Terminate();
		};
		_dialogPage.Navigate(taskDialogPage);
		_dialogPage = taskDialogPage;
	}

	public override void CloseBootstrapper()
	{
		if (base.InvokeRequired)
		{
			Invoke(CloseBootstrapper);
			return;
		}
		_dialogPage.BoundDialog?.Close();
		base.CloseBootstrapper();
	}

	private void VistaDialog_Load(object sender, EventArgs e)
	{
		TaskDialog.ShowDialog(_dialogPage);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(0, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "VistaDialog";
		base.Opacity = 0.0;
		base.ShowInTaskbar = false;
		this.Text = "VistaDialog";
		base.WindowState = System.Windows.Forms.FormWindowState.Minimized;
		base.Load += new System.EventHandler(VistaDialog_Load);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(base.Dialog_FormClosing);
		base.ResumeLayout(false);
	}
}
