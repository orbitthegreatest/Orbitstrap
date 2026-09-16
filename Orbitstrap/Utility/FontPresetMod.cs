using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Orbitstrap
{
    /// <summary>
    /// Downloads and applies font presets from the Orbitstrap-things external assets
    /// repo's fonts/manifest.json. Each preset is a font file that gets copied to the
    /// Roblox Mods folder as a custom font.
    /// </summary>
    public static class FontPresetMod
    {
        private const string ManifestUrl =
            "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/fonts/manifest.json";

        public class ManifestEntry
        {
            [System.Text.Json.Serialization.JsonPropertyName("id")]
            public string Id { get; set; } = "";
            [System.Text.Json.Serialization.JsonPropertyName("name")]
            public string Name { get; set; } = "";
            [System.Text.Json.Serialization.JsonPropertyName("url")]
            public string Url { get; set; } = "";
            [System.Text.Json.Serialization.JsonPropertyName("preview")]
            public string Preview { get; set; } = "";

            public ManifestEntry() { }
            public ManifestEntry(string id, string name, string url, string preview)
            {
                Id = id; Name = name; Url = url; Preview = preview;
            }
        }

        public static async Task<List<ManifestEntry>> GetManifestAsync()
        {
            return await Orbitstrap.Utility.Http.GetJson<List<ManifestEntry>>(ManifestUrl);
        }

        /// <summary>
        /// Downloads the font file and copies it to Paths.CustomFont.
        /// </summary>
        public static async Task ApplyAsync(string url)
        {
            string callId = Guid.NewGuid().ToString("N");
            string tempFile = Path.Combine(Path.GetTempPath(), $"orbitstrap-font-{callId}");

            try
            {
                var data = await App.HttpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempFile, data);

                string? directoryPath = Path.GetDirectoryName(Paths.CustomFont);
                if (directoryPath != null)
                    Directory.CreateDirectory(directoryPath);

                Filesystem.AssertReadOnly(Paths.CustomFont);
                File.Copy(tempFile, Paths.CustomFont, true);
            }
            finally
            {
                try { File.Delete(tempFile); } catch { }
            }
        }

        /// <summary>
        /// Removes the custom font, restoring vanilla Roblox fonts.
        /// </summary>
        public static void Remove()
        {
            try
            {
                if (File.Exists(Paths.CustomFont))
                {
                    Filesystem.AssertReadOnly(Paths.CustomFont);
                    File.Delete(Paths.CustomFont);
                }
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("FontPresetMod::Remove", ex);
            }
        }

        public static void ApplyDefault()
        {
            Remove();
        }
    }
}
