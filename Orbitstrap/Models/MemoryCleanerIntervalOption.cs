namespace Orbitstrap.Models;

/// <summary>Represents a named memory-cleaner interval for display in the settings ComboBox.</summary>
public class MemoryCleanerIntervalOption
{
    public string Name { get; }
    public int Seconds { get; }

    public MemoryCleanerIntervalOption(string name, int seconds)
    {
        Name = name;
        Seconds = seconds;
    }

    public static readonly MemoryCleanerIntervalOption[] All =
    {
        new("Never",        0),
        new("Every 30s",    30),
        new("Every 1 min",  60),
        new("Every 5 min",  300),
        new("Every 10 min", 600),
        new("Every 30 min", 1800),
    };
}
