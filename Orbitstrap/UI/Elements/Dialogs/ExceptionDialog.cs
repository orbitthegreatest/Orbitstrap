using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Media;
using System.Web;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class ExceptionDialog : WpfUiWindow, IComponentConnector
{
	private const int MAX_GITHUB_URL_LENGTH = 8192;

	public ExceptionDialog(Exception exception)
	{
		InitializeComponent();
		AddException(exception);
		if (!App.Logger.Initialized)
		{
			LocateLogFileButton.Content = Strings.Dialog_Exception_CopyLogContents;
		}
		string text = "https://github.com/Orbitstrap";
		string arg = text + "/wiki";
		string text2 = HttpUtility.UrlEncode($"[BUG] {exception.GetType()}: {exception.Message}");
		string value = HttpUtility.UrlEncode(App.Logger.AsDocument);
		string issueUrl = $"{text}/issues/new?template=bug_report.yaml&title={text2}&log={value}";
		if (issueUrl.Length > 8192)
		{
			issueUrl = text + "/issues/new?template=bug_report.yaml&title=" + text2;
			if (issueUrl.Length > 8192)
			{
				issueUrl = text + "/issues/new?template=bug_report.yaml";
			}
		}
		string markdownText = string.Format(Strings.Dialog_Exception_Info_2, arg, issueUrl);
		if (!App.IsActionBuild && !App.BuildMetadata.Machine.Contains("pizzaboxer", StringComparison.Ordinal))
		{
			markdownText = string.Format(Strings.Dialog_Exception_Info_2_Alt, arg);
		}
		HelpMessageMDTextBlock.MarkdownText = markdownText;
		VersionText.Text = string.Format(Strings.Dialog_Exception_Version, App.Version);
		ReportExceptionButton.Click += delegate
		{
			Utilities.ShellExecute(issueUrl);
		};
		LocateLogFileButton.Click += delegate
		{
			if (App.Logger.Initialized && !string.IsNullOrEmpty(App.Logger.FileLocation))
			{
				Utilities.ShellExecute(App.Logger.FileLocation);
			}
			else
			{
				Clipboard.SetDataObject(App.Logger.AsDocument);
			}
		};
		CloseButton.Click += delegate
		{
			Close();
		};
		SystemSounds.Hand.Play();
		base.Loaded += delegate
		{
			PInvoke.FlashWindow((HWND)new WindowInteropHelper(this).Handle, true);
		};
	}

	private void AddException(Exception exception, bool inner = false)
	{
		if (!inner)
		{
			ErrorRichTextBox.Selection.Text = $"{exception.GetType()}: {exception.Message}";
		}
		if (exception.InnerException != null)
		{
			ErrorRichTextBox.Selection.Text += $"\n\n[Inner Exception]\n{exception.InnerException.GetType()}: {exception.InnerException.Message}";
			AddException(exception.InnerException, inner: true);
		}
	}
}
