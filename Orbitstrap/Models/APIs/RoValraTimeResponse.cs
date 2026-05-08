using System.Collections.Generic;
using System.Text.Json.Serialization;
using Orbitstrap.Models.APIs.RoValra;

namespace Orbitstrap.Models.APIs;

public class RoValraTimeResponse
{
	[JsonPropertyName("status")]
	public string Status;

	[JsonPropertyName("servers")]
	public List<RoValraServer>? Servers { get; set; }
}
