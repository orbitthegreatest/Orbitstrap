using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orbitstrap.Utility
{
    /// <summary>
    /// Applies or reverts the Korblox Right Leg and Headless mods.
    ///
    /// Both mods work by writing mesh files directly into the active Roblox version directory at
    /// launch time (called from Bootstrapper.ApplyModifications), not through the Mods folder
    /// staging pipeline — because the pipeline explicitly skips .mesh files.
    ///
    /// Meshes are downloaded from orbitthegreatest/Headless-Korblox-in-R6 on GitHub and cached
    /// at Paths.Cache\KorbloxHeadless\ so subsequent launches are instant (no re-download).
    ///
    /// Korblox Right Leg — replaces content\avatar\meshes\rightleg.mesh with the Korblox leg mesh.
    /// Headless          — empties content\avatar\heads\ so Roblox renders no head.
    ///
    /// Revert for both mods restores the original meshes from a separately cached default set
    /// (also from the same GitHub repo).
    /// </summary>
    public static class KorbloxHeadlessMod
    {
        // ── GitHub raw URLs (spaces are valid in raw.githubusercontent.com URLs) ──
        private const string RawBase    = "https://raw.githubusercontent.com/orbitthegreatest/Headless-Korblox-in-R6/main";
        private const string KorbloxUrl = RawBase + "/Korblox%20Meshes/rightleg.mesh";
        private const string DefMeshUrl = RawBase + "/default%20avatar%20meshes/default%20meshes/rightleg.mesh";
        private const string DefHeadBase = RawBase + "/default%20avatar%20meshes/default%20head%20meshes";

        // All default head mesh filenames (matching the .bat file's list)
        private static readonly string[] DefaultHeadNames =
        {
            "head", "headA", "headB", "headC", "headD", "headE", "headF", "headG",
            "headH", "headI", "headJ", "headK", "headL", "headM", "headN", "headO", "headP"
        };

        // ── Cache folders ───────────────────────────────────────────────────────
        private static string CacheRoot       => Path.Combine(Paths.Cache, "KorbloxHeadless");
        private static string CacheKorblox    => Path.Combine(CacheRoot, "korblox");
        private static string CacheDefMeshes  => Path.Combine(CacheRoot, "default_meshes");
        private static string CacheDefHeads   => Path.Combine(CacheRoot, "default_heads");

        // ── Roblox destination paths (relative to the version directory) ───────
        private const string RelMeshesDir = @"content\avatar\meshes";
        private const string RelHeadsDir  = @"content\avatar\heads";

        // ════════════════════════════════════════════════════════════════════════
        //  PUBLIC API — called from Bootstrapper.ApplyModifications
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Applies the Korblox right-leg mesh to the given Roblox version directory.
        /// Downloads and caches the mesh on first call; subsequent calls are instant.
        /// </summary>
        public static async Task ApplyKorbloxRightLegAsync(string versionDir)
        {
            Directory.CreateDirectory(CacheKorblox);
            string cached = Path.Combine(CacheKorblox, "rightleg.mesh");

            if (!File.Exists(cached))
            {
                App.Logger.WriteLine("KorbloxHeadlessMod::Apply", "Downloading Korblox right-leg mesh…");
                await DownloadFileAsync(KorbloxUrl, cached);
            }

            string dest = Path.Combine(versionDir, RelMeshesDir, "rightleg.mesh");
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(cached, dest, overwrite: true);
            App.Logger.WriteLine("KorbloxHeadlessMod::Apply", "Korblox right-leg mesh applied.");
        }

        /// <summary>
        /// Restores the original right-leg mesh in the given Roblox version directory.
        /// Downloads and caches the default mesh on first call.
        /// </summary>
        public static async Task RevertKorbloxRightLegAsync(string versionDir)
        {
            Directory.CreateDirectory(CacheDefMeshes);
            string cached = Path.Combine(CacheDefMeshes, "rightleg.mesh");

            if (!File.Exists(cached))
            {
                App.Logger.WriteLine("KorbloxHeadlessMod::Revert", "Downloading default right-leg mesh…");
                await DownloadFileAsync(DefMeshUrl, cached);
            }

            string dest = Path.Combine(versionDir, RelMeshesDir, "rightleg.mesh");
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(cached, dest, overwrite: true);
            App.Logger.WriteLine("KorbloxHeadlessMod::Revert", "Default right-leg mesh restored.");
        }

        /// <summary>
        /// Applies the Headless mod by emptying the heads folder in the given Roblox version
        /// directory. The folder itself is kept so Roblox doesn't recreate it with defaults.
        /// </summary>
        public static void ApplyHeadless(string versionDir)
        {
            string headsDir = Path.Combine(versionDir, RelHeadsDir);
            if (!Directory.Exists(headsDir))
            {
                App.Logger.WriteLine("KorbloxHeadlessMod::ApplyHeadless", "Heads folder not found — skipping.");
                return;
            }

            foreach (string file in Directory.GetFiles(headsDir))
            {
                try { File.Delete(file); }
                catch (Exception ex)
                {
                    App.Logger.WriteException("KorbloxHeadlessMod::ApplyHeadless", ex);
                }
            }

            App.Logger.WriteLine("KorbloxHeadlessMod::ApplyHeadless", "Heads folder cleared (Headless applied).");
        }

        /// <summary>
        /// Restores all default head meshes to the given Roblox version directory.
        /// Downloads and caches them on first call.
        /// </summary>
        public static async Task RevertHeadlessAsync(string versionDir)
        {
            Directory.CreateDirectory(CacheDefHeads);

            // Download any missing default heads into the cache
            foreach (string name in DefaultHeadNames)
            {
                string cached = Path.Combine(CacheDefHeads, $"{name}.mesh");
                if (!File.Exists(cached))
                {
                    App.Logger.WriteLine("KorbloxHeadlessMod::RevertHeadless", $"Downloading default head: {name}.mesh…");
                    string url = $"{DefHeadBase}/{name}.mesh";
                    await DownloadFileAsync(url, cached);
                }
            }

            // Copy cached defaults into the Roblox version heads folder
            string headsDir = Path.Combine(versionDir, RelHeadsDir);
            Directory.CreateDirectory(headsDir);

            foreach (string name in DefaultHeadNames)
            {
                string cached = Path.Combine(CacheDefHeads, $"{name}.mesh");
                if (!File.Exists(cached)) continue; // skip if download failed silently

                string dest = Path.Combine(headsDir, $"{name}.mesh");
                try { File.Copy(cached, dest, overwrite: true); }
                catch (Exception ex)
                {
                    App.Logger.WriteException("KorbloxHeadlessMod::RevertHeadless", ex);
                }
            }

            App.Logger.WriteLine("KorbloxHeadlessMod::RevertHeadless", "Default head meshes restored.");
        }

        /// <summary>
        /// Clears the local cache so meshes are re-downloaded on the next launch.
        /// </summary>
        public static void ClearCache()
        {
            if (Directory.Exists(CacheRoot))
                Directory.Delete(CacheRoot, recursive: true);
            App.Logger.WriteLine("KorbloxHeadlessMod::ClearCache", "Mesh cache cleared.");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════════

        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using var response = await App.HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            using var stream = await response.Content.ReadAsStreamAsync();
            using var file   = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(file);
        }
    }
}
