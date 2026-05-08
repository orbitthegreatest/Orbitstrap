using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Orbitstrap.UI.Elements.Base;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class ConnectivityDialog : WpfUiWindow, IComponentConnector
{
	public ConnectivityDialog(string title, string description, MessageBoxImage image, Exception exception)
	{
		InitializeComponent();
		string text = null;
		SystemSound systemSound = null;
		switch (image)
		{
		case MessageBoxImage.Hand:
			text = "Error";
			systemSound = SystemSounds.Hand;
			break;
		case MessageBoxImage.Question:
			text = "Question";
			systemSound = SystemSounds.Question;
			break;
		case MessageBoxImage.Exclamation:
			text = "Warning";
			systemSound = SystemSounds.Exclamation;
			break;
		case MessageBoxImage.Asterisk:
			text = "Information";
			systemSound = SystemSounds.Asterisk;
			break;
		}
		if (text == null)
		{
			IconImage.Visibility = Visibility.Collapsed;
		}
		else
		{
			IconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/MessageBox/" + text + ".png"));
		}
		TitleTextBlock.Text = title;
		DescriptionTextBlock.MarkdownText = description;
		AddException(exception);
		CloseButton.Click += delegate
		{
			Close();
		};
		systemSound?.Play();
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
