using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Orbitstrap
{
    /// <summary>
    /// Downloads and applies a custom emote wheel from the "Orbitstrap-things" external assets
    /// repo's emote-wheels/manifest.json (Task 2B/3B). Mirrors the download-and-extract pattern
    /// already used by DarkTexturesInstaller, and the "only remove what I wrote" pattern already
    /// used by BlackCursorMod — a small tracker file records exactly which files under the
    /// destination folder belong to the currently-applied wheel, so switching wheels (or picking
    /// "None") only ever touches files this mod itself wrote, never anything else that might
    /// already live under content/textures/ui/Emotes/Large/.
    /// </summary>
    public static class EmoteWheelMod
    {
        private const string ManifestUrl =
            "https://raw.githubusercontent.com/orbitthegreatest/Orbitstrap-things/main/emote-wheels/manifest.json";

        // BUG FIX (reported by user): this used to be { "content", "gui", "EmotesMenu" }, which
        // extracted each wheel's zip into Paths.Mods\content\gui\EmotesMenu\. But the zips
        // already contain their own "content\textures\ui\Emotes\Large\..." folder structure
        // internally (that's the actual path Roblox reads emote wheel button icons from), so the
        // old code was nesting that structure a level too deep:
        // Paths.Mods\content\gui\EmotesMenu\content\textures\ui\Emotes\Large\... — a path Roblox
        // never reads, so nothing ever actually changed in-game no matter which wheel was
        // selected, and the previously-applied wheel's files (wherever they'd last landed, e.g.
        // Cute Bears from an early test) kept being whatever Roblox was still using. The
        // destination is now Paths.Mods root, so each wheel's own internal
        // content\textures\ui\Emotes\Large\ structure lands exactly where Roblox expects it.
        private static readonly string[] DestRelativeParts = Array.Empty<string>();
        private const string TrackerFileName = ".orbitstrap_emotewheel_files.json";

        // BUG FIX (reported by user): this internal tracker file was being written straight into
        // the user-visible Mods folder (Paths.Mods), showing up as a stray, confusing file
        // alongside real mods. It's purely internal bookkeeping — same category as other internal
        // caches like GameHistory.json — so it now lives in Paths.Cache instead.
        private static string TrackerPath
        {
            get
            {
                string newPath = Path.Combine(Paths.Cache, TrackerFileName);
                string oldPath = Path.Combine(Paths.Mods, TrackerFileName);

                // One-time migration: if an old tracker file exists from a previous version and
                // nothing has been written to the new location yet, move it over so a currently
                // applied wheel doesn't get orphaned (unremovable/un-switchable) after updating.
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

        public record ManifestEntry(
            [property: JsonPropertyName("id")] string Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("url")] string Url);

        /// <summary>Fetches the list of available emote wheels from the Orbitstrap-things repo.</summary>
        public static async Task<List<ManifestEntry>> GetManifestAsync()
        {
            return await Orbitstrap.Utility.Http.GetJson<List<ManifestEntry>>(ManifestUrl);
        }

        /// <summary>Downloads the zip at <paramref name="url"/> and extracts it into the Mods
        /// root (each wheel zip already contains its own content\textures\ui\Emotes\Large\...
        /// structure), replacing whatever wheel (if any) was applied before.</summary>
        public static async Task ApplyAsync(string url)
        {
            Remove();

            string destPath = Path.Combine(Paths.Mods, Path.Combine(DestRelativeParts));
            Directory.CreateDirectory(destPath);

            // Unique-per-call temp names: two overlapping ApplyAsync calls (e.g. the user changing
            // the dropdown twice in quick succession) must never be able to read or write each
            // other's temp files, or one wheel's files can end up mixed into another's.
            string callId = Guid.NewGuid().ToString("N");
            string tempZip = Path.Combine(Path.GetTempPath(), $"orbitstrap-emotewheel-{callId}.zip");
            string tempExtract = Path.Combine(Path.GetTempPath(), $"orbitstrap-emotewheel-extract-{callId}");
            if (Directory.Exists(tempExtract))
                Directory.Delete(tempExtract, true);

            var data = await App.HttpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(tempZip, data);

            ZipFile.ExtractToDirectory(tempZip, tempExtract);

            // Some source zips have a single top-level folder wrapping the actual files, some don't.
            string sourceRoot = tempExtract;
            var topEntries = Directory.GetFileSystemEntries(tempExtract);
            if (topEntries.Length == 1 && Directory.Exists(topEntries[0]))
                sourceRoot = topEntries[0];

            var writtenRelativePaths = new List<string>();
            foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceRoot, file);
                string dest = Path.Combine(destPath, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, true);
                writtenRelativePaths.Add(relative);
            }

            Directory.CreateDirectory(Paths.Cache);
            File.WriteAllText(
                TrackerPath,
                JsonSerializer.Serialize(writtenRelativePaths));

            File.Delete(tempZip);
            Directory.Delete(tempExtract, true);
        }

        /// <summary>Removes only the files this mod wrote for the currently-applied wheel
        /// (tracked via the tracker file), restoring the vanilla emote wheel textures.</summary>
        public static void Remove()
        {
            string trackerPath = TrackerPath;
            if (!File.Exists(trackerPath))
                return;

            try
            {
                var relativePaths = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(trackerPath))
                    ?? new List<string>();
                string destPath = Path.Combine(Paths.Mods, Path.Combine(DestRelativeParts));

                foreach (var relative in relativePaths)
                {
                    string file = Path.Combine(destPath, relative);
                    if (File.Exists(file))
                        File.Delete(file);
                }

                File.Delete(trackerPath);
            }
            catch (Exception ex)
            {
                App.Logger?.WriteException("EmoteWheelMod::Remove", ex);
            }
        }
    }
}
