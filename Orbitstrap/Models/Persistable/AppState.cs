using System.Collections.Generic;

namespace Orbitstrap.Models.Persistable;

public class AppState
{
	public string VersionGuid { get; set; } = string.Empty;

	public Dictionary<string, string> PackageHashes { get; set; } = new Dictionary<string, string>();

	public int Size { get; set; }
}
