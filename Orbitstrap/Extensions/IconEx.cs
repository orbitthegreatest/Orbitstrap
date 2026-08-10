using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Orbitstrap.Extensions
{
    public static class IconEx
    {
        // IMPORTANT: always call GetSized(...) before GetImageSource() when the icon will be
        // displayed larger than ~32px (bootstrapper dialogs, splash screens, etc). A
        // System.Drawing.Icon loaded from a multi-size .ico via the default Icon(string)/
        // resx constructor resolves to the *system's small icon size* (commonly 32x32), not
        // the largest frame in the file, even though bigger frames exist in the .ico. Any
        // Image control bound to that Icon's ImageSource at a larger size (e.g. the 80x80
        // logo on the "Starting Roblox" bootstrapper dialogs) ends up upscaling that 32x32
        // bitmap, which is what caused the reported blurry launch-dialog logo. GetSized
        // requests a specific frame/size from the .ico so the result is sharp at the size
        // it's actually displayed at.
        public static Icon GetSized(this Icon icon, int width, int height) => new(icon, new Size(width, height));

        public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
        {
            using MemoryStream stream = new();
            icon.Save(stream);

            if (handleException)
            {
                try
                {
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("IconEx::GetImageSource", ex);
                    Frontend.ShowMessageBox(String.Format(Strings.Dialog_IconLoadFailed, ex.Message));
                    return BootstrapperIcon.IconOrbitstrap.GetIcon().GetImageSource(false);
                }
            }
            else
            {
                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}
