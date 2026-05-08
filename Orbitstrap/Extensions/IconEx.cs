using System;
using System.Drawing;
using System.IO;
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
		return new Icon(icon, new Size(width, height));
	}

	public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
	{
		using MemoryStream memoryStream = new MemoryStream();
		icon.Save(memoryStream);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		if (handleException)
		{
			try
			{
				return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
			}
			catch (Exception ex)
			{
				App.Logger.WriteException("IconEx::GetImageSource", ex);
				Frontend.ShowMessageBox(string.Format(Strings.Dialog_IconLoadFailed, ex.Message));
				return BootstrapperIcon.IconOrbitstrap.GetIcon().GetImageSource(handleException: false);
			}
		}
		return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
	}
}
