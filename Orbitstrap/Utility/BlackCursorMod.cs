using System;
using System.IO;
using System.Windows;

namespace Orbitstrap
{
    /// <summary>
    /// Applies or removes the built-in "Black Cursor" mod. Unlike DarkTexturesInstaller, this
    /// mod's assets are bundled with the app (Resources\BlackCursor\*.png, embedded as WPF
    /// pack resources) so no download is required — it's just a copy into the Mods folder.
    ///
    /// It writes to the same paths the "Custom Cursor Set" feature uses
    /// (Mods\content\textures\... ), so the two are mutually exclusive by nature: turning one
    /// on will overwrite whatever the other last wrote. That's expected/acceptable for a simple
    /// built-in preset, but Remove() only deletes the specific files this mod owns rather than
    /// the whole folder, so it doesn't clobber anything else that might share the directory.
    /// </summary>
    public static class BlackCursorMod
    {
        // (resource name under Resources/BlackCursor, destination path relative to Paths.Mods)
        private static readonly (string ResourceName, string[] RelativeDestParts)[] Files = new[]
        {
            ("MouseLockedCursor.png", new[] { "content", "textures", "MouseLockedCursor.png" }),
            ("ArrowCursor.png",       new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowCursor.png" }),
            ("ArrowFarCursor.png",    new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowFarCursor.png" }),
            ("IBeamCursor.png",       new[] { "content", "textures", "Cursors", "KeyboardMouse", "IBeamCursor.png" }),
            ("CrossMouseIcon.png",    new[] { "content", "textures", "Cursors", "CrossMouseIcon.png" }),
        };

        public static void Apply()
        {
            foreach (var (resourceName, relativeDestParts) in Files)
            {
                string destPath = Path.Combine(Paths.Mods, Path.Combine(relativeDestParts));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                var uri = new Uri($"pack://application:,,,/Resources/BlackCursor/{resourceName}");
                var resourceInfo = Application.GetResourceStream(uri)
                    ?? throw new FileNotFoundException($"Embedded Black Cursor resource not found: {resourceName}");

                using var resStream = resourceInfo.Stream;
                using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
                resStream.CopyTo(fileStream);
            }
        }

        public static void Remove()
        {
            foreach (var (_, relativeDestParts) in Files)
            {
                string destPath = Path.Combine(Paths.Mods, Path.Combine(relativeDestParts));
                try
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                }
                catch (Exception ex)
                {
                    App.Logger?.WriteException("BlackCursorMod::Remove", ex);
                }
            }
        }
    }
}
