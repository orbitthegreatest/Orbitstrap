using System.Text.Json.Serialization;
using Wpf.Ui.Controls;

namespace Orbitstrap.Models.APIs.Config;

public class RemoteDataBase
{
	[JsonPropertyName("alertEnabled")]
	public bool AlertEnabled { get; set; }

	[JsonPropertyName("alertContent")]
	public string AlertContent { get; set; }

	[JsonPropertyName("alertSeverity")]
	public InfoBarSeverity AlertSeverity { get; set; } = InfoBarSeverity.Warning;

	[JsonPropertyName("killFlags")]
	public bool KillFlags { get; set; }

	[JsonPropertyName("deeplinkUrl")]
	public string DeeplinkUrl { get; set; } = "https://Orbitstrap.app/joingame";

	[JsonPropertyName("packageMaps")]
	public PackageMaps PackageMaps { get; set; } = new PackageMaps();
}
