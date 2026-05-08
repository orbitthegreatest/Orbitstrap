using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Bootstrapper.Base;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public class LegacyDialog2008 : WinFormsDialogBase
{
	private IContainer components = null!;

	private Label labelMessage;

	private ProgressBar ProgressBar;

	private System.Windows.Forms.Button buttonCancel;

	protected override string _message
	{
		get
		{
			return labelMessage.Text;
		}
		set
		{
			labelMessage.Text = value;
		}
	}

	protected override ProgressBarStyle _progressStyle
	{
		get
		{
			return ProgressBar.Style;
		}
		set
		{
			ProgressBar.Style = value;
		}
	}

	protected override int _progressMaximum
	{
		get
		{
			return ProgressBar.Maximum;
		}
		set
		{
			ProgressBar.Maximum = value;
		}
	}

	protected override int _progressValue
	{
		get
		{
			return ProgressBar.Value;
		}
		set
		{
			ProgressBar.Value = value;
		}
	}

	protected override bool _cancelEnabled
	{
		get
		{
			return buttonCancel.Enabled;
		}
		set
		{
			buttonCancel.Enabled = value;
		}
	}

	public LegacyDialog2008()
	{
		InitializeComponent();
		buttonCancel.Text = Strings.Common_Cancel;
		ScaleWindow();
		SetupDialog();
		ProgressBar.RightToLeft = RightToLeft;
		ProgressBar.RightToLeftLayout = RightToLeftLayout;
	}

	private void LegacyDialog2008_Load(object sender, EventArgs e)
	{
		Activate();
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
		this.labelMessage = new System.Windows.Forms.Label();
		this.ProgressBar = new System.Windows.Forms.ProgressBar();
		this.buttonCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.labelMessage.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.labelMessage.Location = new System.Drawing.Point(12, 16);
		this.labelMessage.Name = "labelMessage";
		this.labelMessage.Size = new System.Drawing.Size(287, 17);
		this.labelMessage.TabIndex = 0;
		this.labelMessage.Text = "Please wait...";
		this.ProgressBar.Location = new System.Drawing.Point(15, 47);
		this.ProgressBar.MarqueeAnimationSpeed = 33;
		this.ProgressBar.Name = "ProgressBar";
		this.ProgressBar.Size = new System.Drawing.Size(281, 20);
		this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.ProgressBar.TabIndex = 1;
		this.buttonCancel.Enabled = false;
		this.buttonCancel.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.buttonCancel.Location = new System.Drawing.Point(221, 83);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 3;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Click += new System.EventHandler(base.ButtonCancel_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(311, 122);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.ProgressBar);
		base.Controls.Add(this.labelMessage);
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(327, 161);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(327, 161);
		base.Name = "LegacyDialog2008";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "LegacyDialog2008";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(base.Dialog_FormClosing);
		base.Load += new System.EventHandler(LegacyDialog2008_Load);
		base.ResumeLayout(false);
	}
}
