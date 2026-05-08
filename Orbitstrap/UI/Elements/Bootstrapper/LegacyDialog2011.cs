using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orbitstrap.Extensions;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Bootstrapper.Base;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public class LegacyDialog2011 : WinFormsDialogBase
{
	private IContainer components = null!;

	private Label labelMessage;

	private ProgressBar ProgressBar;

	private PictureBox IconBox;

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
			System.Windows.Forms.Button button = buttonCancel;
			bool enabled = (buttonCancel.Visible = value);
			button.Enabled = enabled;
		}
	}

	public LegacyDialog2011()
	{
		InitializeComponent();
		IconBox.BackgroundImage = App.Settings.Prop.BootstrapperIcon.GetIcon().ToBitmap();
		buttonCancel.Text = Strings.Common_Cancel;
		ScaleWindow();
		SetupDialog();
		ProgressBar.RightToLeft = RightToLeft;
		ProgressBar.RightToLeftLayout = RightToLeftLayout;
	}

	private void LegacyDialog2011_Load(object sender, EventArgs e)
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
		this.IconBox = new System.Windows.Forms.PictureBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.IconBox).BeginInit();
		base.SuspendLayout();
		this.labelMessage.Location = new System.Drawing.Point(55, 23);
		this.labelMessage.Name = "labelMessage";
		this.labelMessage.Size = new System.Drawing.Size(287, 17);
		this.labelMessage.TabIndex = 0;
		this.labelMessage.Text = "Please wait...";
		this.ProgressBar.Location = new System.Drawing.Point(58, 51);
		this.ProgressBar.MarqueeAnimationSpeed = 33;
		this.ProgressBar.Name = "ProgressBar";
		this.ProgressBar.Size = new System.Drawing.Size(287, 26);
		this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.ProgressBar.TabIndex = 1;
		this.IconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.IconBox.ImageLocation = "";
		this.IconBox.Location = new System.Drawing.Point(19, 16);
		this.IconBox.Name = "IconBox";
		this.IconBox.Size = new System.Drawing.Size(32, 32);
		this.IconBox.TabIndex = 2;
		this.IconBox.TabStop = false;
		this.buttonCancel.Enabled = false;
		this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.buttonCancel.Location = new System.Drawing.Point(271, 83);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 3;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.buttonCancel.Visible = false;
		this.buttonCancel.Click += new System.EventHandler(base.ButtonCancel_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(362, 131);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.IconBox);
		base.Controls.Add(this.ProgressBar);
		base.Controls.Add(this.labelMessage);
		this.Font = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(378, 170);
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(378, 170);
		base.Name = "LegacyDialog2011";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "LegacyDialog2011";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(base.Dialog_FormClosing);
		base.Load += new System.EventHandler(LegacyDialog2011_Load);
		((System.ComponentModel.ISupportInitialize)this.IconBox).EndInit();
		base.ResumeLayout(false);
	}
}
