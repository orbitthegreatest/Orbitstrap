using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orbitstrap.Enums;
using Orbitstrap.Resources;
using Orbitstrap.UI;

namespace Orbitstrap.Extensions;

public static class IconEx
{
	public static Icon GetSized(this Icon icon, int width, int height)
	{
		return new Icon(icon, new System.Drawing.Size(width, height));
	}

	public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
	{
		if (handleException)
		{
			try
			{
				return CreateSharp(icon);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("IconEx::GetImageSource", ex);
				Frontend.ShowMessageBox(string.Format(Strings.Dialog_IconLoadFailed, ex.Message));
				return BootstrapperIcon.IconOrbitstrap.GetIcon().GetImageSource(handleException: false);
			}
		}
		return CreateSharp(icon);
	}

	/// <summary>
	/// Converts an Icon to a WPF ImageSource without blurring.
	/// BitmapFrame.Create on a saved .ico stream picks only one frame and
	/// WPF then scales it up with anti-aliasing — producing a blurry result.
	/// Imaging.CreateBitmapSourceFromHIcon reads the GDI handle directly so
	/// Windows picks the right frame size and the result stays crisp.
	/// </summary>
	private static ImageSource CreateSharp(Icon icon)
	{
		BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
			icon.Handle,
			Int32Rect.Empty,
			BitmapSizeOptions.FromEmptyOptions());
		bitmapSource.Freeze();
		return bitmapSource;
	}
}
