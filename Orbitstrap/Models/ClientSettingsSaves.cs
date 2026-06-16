using System;
using System.IO;
using System.Text.Json;
using Orbitstrap;

public static class OrbitstrapRobloxSettingsManager // lowk didnt know what tf to name this file
{
    public class OrbitstrapRobloxSettings
    {
        public int MemoryCleanerIntervalSeconds { get; set; }
    }

    private static readonly string FolderPath = Paths.Base;

    private static readonly string FilePath =
        Path.Combine(FolderPath, "OrbitstrapRobloxSaves.json");

    public static OrbitstrapRobloxSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return new OrbitstrapRobloxSettings();

            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<OrbitstrapRobloxSettings>(json)
                   ?? new OrbitstrapRobloxSettings();
        }
        catch
        {
            return new OrbitstrapRobloxSettings();
        }
    }

    public static void Save(OrbitstrapRobloxSettings settings)
    {
        try
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
        }
    }
}
