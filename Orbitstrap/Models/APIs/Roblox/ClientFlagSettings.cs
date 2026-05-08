using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Orbitstrap.Models.APIs.Roblox;

public class ClientFlagSettings
{
	[JsonPropertyName("applicationSettings")]
	public Dictionary<string, string>? ApplicationSettings { get; set; }
}
