using Orbitstrap.Models.Persistable;

namespace Orbitstrap.AppData;

public class RobloxPlayerData : CommonAppData, IAppData
{
	public string ProductName => "Roblox";

	public string BinaryType => "WindowsPlayer";

	public string RegistryName => "RobloxPlayer";

	public override string ExecutableName => "RobloxPlayerBeta.exe";

	public override AppState State => App.RobloxState.Prop.Player;
}
