using System.IO;
using System.Linq;

namespace Orbitstrap.Models.Entities;

public class ModPresetFileData
{
    public string FilePath { get; private set; }

    public string FullFilePath => Path.Combine(Paths.Modifications, FilePath);

    public FileStream FileStream => File.OpenRead(FullFilePath);

    public string ResourceIdentifier { get; private set; }

    public Stream? ResourceStream => Resource.GetStream(ResourceIdentifier);

    public byte[] ResourceHash { get; private set; }

    public ModPresetFileData(string contentPath, string resource)
    {
        FilePath = contentPath;
        ResourceIdentifier = resource;
        using Stream? inputStream = ResourceStream;
        // If the embedded resource is missing, store an empty hash — the preset simply won't apply
        ResourceHash = inputStream != null
            ? App.MD5Provider.ComputeHash(inputStream)
            : Array.Empty<byte>();
    }

    public bool HashMatches()
    {
        if (ResourceHash.Length == 0) return false; // missing resource — treat as never matching
        if (!File.Exists(FullFilePath)) return false;
        using FileStream inputStream = FileStream;
        return App.MD5Provider.ComputeHash(inputStream).SequenceEqual(ResourceHash);
    }
}
