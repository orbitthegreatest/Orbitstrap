using System.IO;
using Orbitstrap.Models.Persistable;

namespace Orbitstrap.AppData;

public abstract class CommonAppData
{
	public virtual string ExecutableName { get; }

	public string Directory => Path.Combine(Paths.Versions, string.IsNullOrEmpty(State.VersionGuid) ? "" : State.VersionGuid);

	public string ExecutablePath => Path.Combine(Directory, ExecutableName);

	public virtual AppState State { get; }
}
