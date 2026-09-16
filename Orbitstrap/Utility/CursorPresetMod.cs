using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Orbitstrap
{
    /// <summary>
    /// Downloads and applies cursor presets from the Orbitstrap-things external assets
    /// repo's cursors/manifest.json. Each preset is a zip containing cursor PNG files
    /// that get mapped to the correct Roblox Mods paths.
    /// </summary>
    public static class CursorPresetMod
    {
        private const string ManifestUrl =
            "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/cursors/manifest.json";

        private const string TrackerFileName = ".orbitstrap_cursor_files.json";

        private static string TrackerPath
        {
            get
            {
                string newPath = Path.Combine(Paths.Cache, TrackerFileName);
                string oldPath = Path.Combine(Paths.Mods, TrackerFileName);

                if (!File.Exists(newPath) && File.Exists(oldPath))
                {
                    try
                    {
                        Directory.CreateDirectory(Paths.Cache);
                        File.Move(oldPath, newPath);
                    }
                    catch
                    {
                        return oldPath;
                    }
                }

                return newPath;
            }
        }

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

        // File mapping: source filename in zip → destination relative to Mods
        private static Dictionary<string, string[]> GetFileMapping()
        {
                return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ArrowCursor.png",          new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowCursor.png" } },
                { "ArrowFarCursor.png",       new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowFarCursor.png" } },
                { "IBeamCursor.png",          new[] { "content", "textures", "Cursors", "KeyboardMouse", "IBeamCursor.png" } },
                { "MouseLockedCursor.png",    new[] { "content", "textures", "MouseLockedCursor.png" } },
            };
        }

        // Mirror files: whatever ArrowCursor.png is, these 3 files get the same content
        private static string[] GetMirrorFileNames()
        {
            return new[]
            {
                "advCursor-default.png",
                "advCursor-white.png",
                "ArrowCursorDecalDrag.png"
            };
        }

        private static IEnumerable<string> GetMirrorDestPaths()
        {
            string texturesDir = Path.Combine(Paths.Mods, "Content", "textures");
            foreach (var name in GetMirrorFileNames())
                yield return Path.Combine(texturesDir, name);
        }

        public static async Task<List<ManifestEntry>> GetManifestAsync()
        {
            return await Orbitstrap.Utility.Http.GetJson<List<ManifestEntry>>(ManifestUrl);
        }

        /// <summary>
        /// Downloads the cursor preset zip and applies it to the Mods folder.
        /// </summary>
        public static async Task ApplyAsync(string url)
        {
            Remove();

            string callId = Guid.NewGuid().ToString("N");
            string tempZip = Path.Combine(Path.GetTempPath(), $"orbitstrap-cursor-{callId}.zip");
            string tempExtract = Path.Combine(Path.GetTempPath(), $"orbitstrap-cursor-extract-{callId}");

            try
            {
                if (Directory.Exists(tempExtract))
                    Directory.Delete(tempExtract, true);

                var data = await App.HttpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempZip, data);

                ZipFile.ExtractToDirectory(tempZip, tempExtract);

                // Some zips have a single top-level folder, some don't
                string sourceRoot = tempExtract;
                var topEntries = Directory.GetFileSystemEntries(tempExtract);
                if (topEntries.Length == 1 && Directory.Exists(topEntries[0]))
                    sourceRoot = topEntries[0];

                var writtenRelativePaths = new List<string>();

                foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
                {
                    string sourceFileName = Path.GetFileName(file);

                    // Map the source file to the correct Mods destination
                    if (GetFileMapping().TryGetValue(sourceFileName, out var destParts))
                    {
                        string destPath = Path.Combine(Paths.Mods, Path.Combine(destParts));
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(file, destPath, true);
                        writtenRelativePaths.Add(Path.Combine(Path.Combine(destParts)));
                    }
                    // Also copy advCursor/ArrowCursorDecalDrag files if they exist in the preset
                    else if (sourceFileName.StartsWith("advCursor", StringComparison.OrdinalIgnoreCase)
                          || sourceFileName.Equals("ArrowCursorDecalDrag.png", StringComparison.OrdinalIgnoreCase))
                    {
                        string destPath = Path.Combine(Paths.Mods, "content", "textures", sourceFileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(file, destPath, true);
                        writtenRelativePaths.Add(Path.Combine("content", "textures", sourceFileName));
                    }
                }

                // Mirror ArrowCursor.png to the 3 advanced cursor files
                string arrowCursorDest = Path.Combine(Paths.Mods, "content", "textures", "Cursors", "KeyboardMouse", "ArrowCursor.png");
                if (File.Exists(arrowCursorDest))
                {
                    foreach (var mirrorDest in GetMirrorDestPaths())
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(mirrorDest)!);
                        File.Copy(arrowCursorDest, mirrorDest, true);
                        string rel = Path.GetRelativePath(Paths.Mods, mirrorDest);
                        if (!writtenRelativePaths.Contains(rel))
                            writtenRelativePaths.Add(rel);
                    }
                }

                Directory.CreateDirectory(Paths.Cache);
                System.IO.File.WriteAllText(
                    TrackerPath,
                    System.Text.Json.JsonSerializer.Serialize(writtenRelativePaths));
            }
            finally
            {
                try { File.Delete(tempZip); } catch { }
                try { Directory.Delete(tempExtract, true); } catch { }
            }
        }

        /// <summary>
        /// Restores vanilla Roblox cursors by removing all custom cursor files applied by presets.
        /// </summary>
        private static readonly string[] DefaultCursorFiles = new[]
        {
            "ArrowCursor.png", "ArrowFarCursor.png", "IBeamCursor.png",
            "MouseLockedCursor.png", "ArrowCursorDecalDrag.png",
            "advCursor-default.png", "advCursor-white.png"
        };

        private static readonly Dictionary<string, string[]> DefaultCursorMapping = new(StringComparer.OrdinalIgnoreCase)
        {
            { "ArrowCursor.png",          new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowCursor.png" } },
            { "ArrowFarCursor.png",       new[] { "content", "textures", "Cursors", "KeyboardMouse", "ArrowFarCursor.png" } },
            { "IBeamCursor.png",          new[] { "content", "textures", "Cursors", "KeyboardMouse", "IBeamCursor.png" } },
            { "MouseLockedCursor.png",    new[] { "content", "textures", "MouseLockedCursor.png" } },
            { "ArrowCursorDecalDrag.png", new[] { "content", "textures", "ArrowCursorDecalDrag.png" } },
            { "advCursor-default.png",    new[] { "content", "textures", "advCursor-default.png" } },
            { "advCursor-white.png",      new[] { "content", "textures", "advCursor-white.png" } },
        };

        private const string DefaultsBaseUrl = "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/defaults/cursors/";

        public static async void ApplyDefault()
        {
            Remove();

            try
            {
                foreach (var fileName in DefaultCursorFiles)
                {
                    if (!DefaultCursorMapping.TryGetValue(fileName, out var destParts))
                        continue;

                    byte[] data = await App.HttpClient.GetByteArrayAsync(DefaultsBaseUrl + fileName);
                    string destPath = Path.Combine(Paths.Mods, Path.Combine(destParts));
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                    await File.WriteAllBytesAsync(destPath, data);
                }
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("CursorPresetMod::ApplyDefault", ex);
            }
        }

        /// <summary>
        /// Removes only the files this mod wrote for the currently-applied cursor preset.
        /// </summary>
        public static void Remove()
        {
            string trackerPath = TrackerPath;
            if (!File.Exists(trackerPath))
                return;

            try
            {
                var relativePaths = System.Text.Json.JsonSerializer.Deserialize<List<string>>(File.ReadAllText(trackerPath))
                    ?? new List<string>();

                foreach (var relative in relativePaths)
                {
                    string file = Path.Combine(Paths.Mods, relative);
                    if (File.Exists(file))
                        File.Delete(file);
                }

                File.Delete(trackerPath);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("CursorPresetMod::Remove", ex);
            }
        }
    }
}
