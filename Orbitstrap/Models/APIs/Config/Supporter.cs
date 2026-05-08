using System.Text.Json.Serialization;

namespace Orbitstrap.Models.APIs.Config;

public class Supporter
{
	[JsonPropertyName("imageAsset")]
	public string ImageAsset { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	public string Image => "https://raw.githubusercontent.com/Orbitstraplabs/config/main/assets/" + ImageAsset;
}
