namespace Orbitstrap
{
    static class Paths
    {
        public static string Temp => Path.Combine(Path.GetTempPath(), App.ProjectName);
        public static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        public static string WindowsStartMenu => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        public static string System => Environment.GetFolderPath(Environment.SpecialFolder.System);
        public static string Process => Environment.ProcessPath!;

        public static string TempUpdates => Path.Combine(Temp, "Updates");
        public static string TempLogs => Path.Combine(Temp, "Logs");

        public static string Cache { get; private set; } = "";
        public static string RobloxLogs { get; private set; } = "";
        public static string RobloxCache { get; private set; } = "";
        public static string RobloxBase { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Orbitstrap");
        public static string Base { get; private set; } = "";
        public static string Downloads { get; private set; } = "";
        public static string SavedBackups { get; private set; } = "";
        public static string Logs { get; private set; } = "";
        public static string Integrations { get; private set; } = "";
        public static string Versions { get; private set; } = "";
        public static string Mods { get; private set; } = "";

        public static string SavedFlagBackup { get; private set; } = "";
        public static string Roblox { get; private set; } = "";
        public static string CustomThemes { get; private set; } = "";
        public static string CustomCursors { get; private set; } = "";

        public static string Application { get; private set; } = "";

        public static string CustomFont => Path.Combine(Mods, "content", "fonts", "CustomFont.ttf");
 
        public static bool Initialized => !string.IsNullOrWhiteSpace(Base);

        public static void Initialize(string baseDirectory, string? robloxBaseDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("Base directory cannot be null or empty.", nameof(baseDirectory));

            Base = baseDirectory;

            Downloads = Path.Combine(Base, "Downloads");
            SavedBackups = Path.Combine(Base, "SavedBackups");
            Logs = Path.Combine(Base, "Logs");
            Integrations = Path.Combine(Base, "Integrations");
            Versions = Path.Combine(RobloxBase, "RblxVersions");
            Mods = Path.Combine(Base, "OrbitstrapMods");
            Roblox = Path.Combine(Base, "Roblox");
            CustomThemes = Path.Combine(Base, "CustomThemes");
            SavedFlagBackup = Path.Combine(Base, "SavedFlagBackup");
            CustomCursors = Path.Combine(Base, "CustomCursorsSets");
            Cache = Path.Combine(Base, "Cache");

            Application = Path.Combine(Base, $"{App.ProjectName}.exe");

            EnsureDirectoryExists(Downloads);
            EnsureDirectoryExists(SavedBackups);
            EnsureDirectoryExists(Logs);
            EnsureDirectoryExists(Integrations);
            EnsureDirectoryExists(Versions);
            EnsureDirectoryExists(Mods);
            EnsureDirectoryExists(CustomThemes);
            // BUG FIX: Cache was defined but never actually created here, so anything that
            // wrote straight into Paths.Cache (e.g. AccountManager.SaveAccounts()) would throw
            // a DirectoryNotFoundException that got silently swallowed by its own try/catch,
            // meaning linked Roblox accounts never actually reached disk and disappeared on
            // every app restart.
            EnsureDirectoryExists(Cache);

            // One-time migration: earlier builds created an empty "Skyboxes" folder (dead code,
            // never actually used — real skybox downloads always went to the separate
            // "SkyboxPack" folder). Clean up the stale empty folder from existing installs.
            string staleSkyboxesFolder = Path.Combine(Base, "Skyboxes");
            if (Directory.Exists(staleSkyboxesFolder))
            {
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(staleSkyboxesFolder).Any())
                        Directory.Delete(staleSkyboxesFolder);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to remove stale Skyboxes folder: {ex.Message}");
                }
            }
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Created missing directory: {directoryPath}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to create directory {directoryPath}: {ex.Message}");
                }
            }
        }
    }
}
