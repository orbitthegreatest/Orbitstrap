using Orbitstrap.Models.Persistable;

namespace Orbitstrap.AppData;

public class RobloxStudioData : CommonAppData, IAppData
{
	public string ProductName => "Roblox Studio";

	public string BinaryType => "WindowsStudio64";

	public string RegistryName => "RobloxStudio";

	public override string ExecutableName => "RobloxStudioBeta.exe";

	public override AppState State => App.RobloxState.Prop.Studio;
}
