using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Orbitstrap
{
    /// <summary>
    /// Downloads and applies death sound presets from the Orbitstrap-things external assets
    /// repo's death-sounds/manifest.json. Each preset is an OGG file that gets copied to
    /// the Roblox Mods folder as the death sound.
    /// </summary>
    public static class DeathSoundPresetMod
    {
        private const string ManifestUrl =
            "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/death-sounds/manifest.json";

        private static string DeathSoundDestPath =>
            Path.Combine(Paths.Mods, "Content", "sounds", "oof.ogg");

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
        /// Downloads the sound file and copies it to the Mods death sound path.
        /// </summary>
        public static async Task ApplyAsync(string url)
        {
            string callId = Guid.NewGuid().ToString("N");
            string tempFile = Path.Combine(Path.GetTempPath(), $"orbitstrap-death-sound-{callId}.ogg");

            try
            {
                var data = await App.HttpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempFile, data);

                Directory.CreateDirectory(Path.GetDirectoryName(DeathSoundDestPath)!);
                File.Copy(tempFile, DeathSoundDestPath, true);
            }
            finally
            {
                try { File.Delete(tempFile); } catch { }
            }
        }

        /// <summary>
        /// Downloads the sound file to a temp location and returns the path for playback.
        /// </summary>
        public static async Task<string> DownloadPreviewAsync(string url)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"orbitstrap-death-sound-preview-{Guid.NewGuid():N}.ogg");
            var data = await App.HttpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(tempFile, data);
            return tempFile;
        }

        /// <summary>
        /// Removes the custom death sound, restoring vanilla Roblox death sound.
        /// </summary>
        public static void Remove()
        {
            try
            {
                if (File.Exists(DeathSoundDestPath))
                    File.Delete(DeathSoundDestPath);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("DeathSoundPresetMod::Remove", ex);
            }
        }

        private const string DefaultOofUrl = "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/defaults/death-sounds/oof.ogg";

        public static async Task ApplyDefault()
        {
            Remove();
            try
            {
                byte[] data = await App.HttpClient.GetByteArrayAsync(DefaultOofUrl);
                Directory.CreateDirectory(Path.GetDirectoryName(DeathSoundDestPath)!);
                await File.WriteAllBytesAsync(DeathSoundDestPath, data);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("DeathSoundPresetMod::ApplyDefault", ex);
            }
        }
    }
}
