using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orbitstrap;

internal static class Resource
{
    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();

    private static readonly string[] resourceNames = assembly.GetManifestResourceNames();

    public static Stream? GetStream(string name)
    {
        string? name2 = resourceNames.FirstOrDefault((string str) => str.EndsWith(name));
        if (name2 == null)
        {
            App.Logger.WriteLine("Resource::GetStream", $"[WARN] Embedded resource not found: {name}");
            return null;
        }
        return assembly.GetManifestResourceStream(name2);
    }

    public static async Task<byte[]> Get(string name)
    {
        using Stream? stream = GetStream(name);
        if (stream == null) return Array.Empty<byte>();
        using MemoryStream memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public static async Task<string> GetString(string name)
    {
        return Encoding.UTF8.GetString(await Get(name));
    }
}
