using System.Text.Json.Serialization;

namespace Orbitstrap.Models.APIs;

public class IPInfoResponse
{
	[JsonPropertyName("city")]
	public string City { get; set; }

	[JsonPropertyName("country")]
	public string Country { get; set; }

	[JsonPropertyName("region")]
	public string Region { get; set; }
}
