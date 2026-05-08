using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Orbitstrap.Enums;
using Orbitstrap.Extensions;
using Orbitstrap.Properties;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Bootstrapper.Base;

namespace Orbitstrap.UI.Elements.Bootstrapper;

public class ProgressDialog : WinFormsDialogBase
{
	private IContainer components = null!;

	private System.Windows.Forms.ProgressBar ProgressBar;

	private Label labelMessage;

	private PictureBox IconBox;

	private Panel panel1;

	private Label buttonCancel;

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
			Label label = buttonCancel;
			bool enabled = (buttonCancel.Visible = value);
			label.Enabled = enabled;
		}
	}

	public ProgressDialog()
	{
		InitializeComponent();
		if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
		{
			labelMessage.ForeColor = SystemColors.Window;
			buttonCancel.ForeColor = System.Drawing.Color.FromArgb(196, 197, 196);
			buttonCancel.Image = Orbitstrap.Properties.Resources.DarkCancelButton;
			panel1.BackColor = System.Drawing.Color.FromArgb(35, 37, 39);
			BackColor = System.Drawing.Color.FromArgb(25, 27, 29);
		}
		labelMessage.Text = Strings.Bootstrapper_StylePreview_TextCancel;
		buttonCancel.Text = Strings.Common_Cancel;
		IconBox.BackgroundImage = App.Settings.Prop.BootstrapperIcon.GetIcon().GetSized(128, 128).ToBitmap();
		SetupDialog();
		ProgressBar.RightToLeft = RightToLeft;
		ProgressBar.RightToLeftLayout = RightToLeftLayout;
	}

	private void ButtonCancel_MouseEnter(object sender, EventArgs e)
	{
		if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
		{
			buttonCancel.Image = Orbitstrap.Properties.Resources.DarkCancelButtonHover;
		}
		else
		{
			buttonCancel.Image = Orbitstrap.Properties.Resources.CancelButtonHover;
		}
	}

	private void ButtonCancel_MouseLeave(object sender, EventArgs e)
	{
		if (App.Settings.Prop.Theme.GetFinal() == Theme.Dark)
		{
			buttonCancel.Image = Orbitstrap.Properties.Resources.DarkCancelButton;
		}
		else
		{
			buttonCancel.Image = Orbitstrap.Properties.Resources.CancelButton;
		}
	}

	private void ProgressDialog_Load(object sender, EventArgs e)
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
		this.ProgressBar = new System.Windows.Forms.ProgressBar();
		this.labelMessage = new System.Windows.Forms.Label();
		this.IconBox = new System.Windows.Forms.PictureBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.buttonCancel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.IconBox).BeginInit();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.ProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.ProgressBar.Location = new System.Drawing.Point(29, 241);
		this.ProgressBar.MarqueeAnimationSpeed = 20;
		this.ProgressBar.Name = "ProgressBar";
		this.ProgressBar.Size = new System.Drawing.Size(460, 20);
		this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.ProgressBar.TabIndex = 0;
		this.labelMessage.Font = new System.Drawing.Font("Tahoma", 11.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.labelMessage.Location = new System.Drawing.Point(29, 199);
		this.labelMessage.Name = "labelMessage";
		this.labelMessage.Size = new System.Drawing.Size(460, 18);
		this.labelMessage.TabIndex = 1;
		this.labelMessage.Text = "Please wait...";
		this.labelMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.labelMessage.UseMnemonic = false;
		this.IconBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
		this.IconBox.ImageLocation = "";
		this.IconBox.Location = new System.Drawing.Point(212, 66);
		this.IconBox.Name = "IconBox";
		this.IconBox.Size = new System.Drawing.Size(92, 92);
		this.IconBox.TabIndex = 2;
		this.IconBox.TabStop = false;
		this.panel1.BackColor = System.Drawing.SystemColors.Window;
		this.panel1.Controls.Add(this.buttonCancel);
		this.panel1.Controls.Add(this.labelMessage);
		this.panel1.Controls.Add(this.IconBox);
		this.panel1.Controls.Add(this.ProgressBar);
		this.panel1.Location = new System.Drawing.Point(1, 1);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(518, 318);
		this.panel1.TabIndex = 4;
		this.buttonCancel.Font = new System.Drawing.Font("Tahoma", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		this.buttonCancel.ForeColor = System.Drawing.Color.FromArgb(75, 75, 75);
		this.buttonCancel.Image = Orbitstrap.Properties.Resources.CancelButton;
		this.buttonCancel.Location = new System.Drawing.Point(194, 264);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(130, 44);
		this.buttonCancel.TabIndex = 4;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.buttonCancel.UseMnemonic = false;
		this.buttonCancel.Click += new System.EventHandler(base.ButtonCancel_Click);
		this.buttonCancel.MouseEnter += new System.EventHandler(ButtonCancel_MouseEnter);
		this.buttonCancel.MouseLeave += new System.EventHandler(ButtonCancel_MouseLeave);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.ActiveBorder;
		base.ClientSize = new System.Drawing.Size(520, 320);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.MaximumSize = new System.Drawing.Size(520, 320);
		this.MinimumSize = new System.Drawing.Size(520, 320);
		base.Name = "ProgressDialog";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "ProgressDialog";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(base.Dialog_FormClosing);
		base.Load += new System.EventHandler(ProgressDialog_Load);
		((System.ComponentModel.ISupportInitialize)this.IconBox).EndInit();
		this.panel1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
