using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Orbitstrap.Resources;
using Orbitstrap.UI.Elements.Base;
using Orbitstrap.UI.Utility;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Orbitstrap.UI.Elements.Dialogs;

public partial class FluentMessageBox : WpfUiWindow, IComponentConnector
{
	public MessageBoxResult Result;

	public FluentMessageBox(string message, MessageBoxImage image, MessageBoxButton buttons)
	{
		InitializeComponent();
		base.Title = "Orbitstrap";
		RootTitleBar.Title = base.Title;
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
		base.Title = "Orbitstrap";
		MessageTextBlock.Text = message;
		MessageTextBlock.MarkdownText = message;
		ButtonOne.Visibility = Visibility.Collapsed;
		ButtonTwo.Visibility = Visibility.Collapsed;
		ButtonThree.Visibility = Visibility.Collapsed;
		switch (buttons)
		{
		case MessageBoxButton.YesNo:
			SetButton(ButtonOne, MessageBoxResult.Yes);
			SetButton(ButtonTwo, MessageBoxResult.No);
			break;
		case MessageBoxButton.YesNoCancel:
			SetButton(ButtonOne, MessageBoxResult.Yes);
			SetButton(ButtonTwo, MessageBoxResult.No);
			SetButton(ButtonThree, MessageBoxResult.Cancel);
			break;
		case MessageBoxButton.OKCancel:
			SetButton(ButtonOne, MessageBoxResult.OK);
			SetButton(ButtonTwo, MessageBoxResult.Cancel);
			break;
		default:
			SetButton(ButtonOne, MessageBoxResult.OK);
			break;
		}
		if (ButtonThree.Visibility == Visibility.Visible)
		{
			base.Width = 356.0;
		}
		else if (ButtonTwo.Visibility == Visibility.Visible)
		{
			base.Width = 245.0;
		}
		double num = Math.Ceiling(Rendering.GetTextWidth(MessageTextBlock));
		num += 40.0;
		if (image != MessageBoxImage.None)
		{
			num += 50.0;
		}
		if (num > base.MaxWidth)
		{
			base.Width = base.MaxWidth;
		}
		else if (num > base.Width)
		{
			base.Width = num;
		}
		systemSound?.Play();
		base.Loaded += delegate
		{
			PInvoke.FlashWindow((HWND)new WindowInteropHelper(this).Handle, true);
		};
	}

	private static string GetTextForResult(MessageBoxResult result)
	{
		return result switch
		{
			MessageBoxResult.OK => Strings.Common_OK, 
			MessageBoxResult.Cancel => Strings.Common_Cancel, 
			MessageBoxResult.Yes => Strings.Common_Yes, 
			MessageBoxResult.No => Strings.Common_No, 
			_ => result.ToString(), 
		};
	}

	public void SetButton(Button button, MessageBoxResult result)
	{
		button.Visibility = Visibility.Visible;
		button.Content = GetTextForResult(result);
		button.Click += delegate
		{
			Result = result;
			Close();
		};
	}
}
