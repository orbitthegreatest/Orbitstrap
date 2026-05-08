using System;
using System.Text.Json.Serialization;

namespace Orbitstrap.Models.RobloxApi;

public class GetUserResponse
{
	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("created")]
	public DateTime Created { get; set; }

	[JsonPropertyName("isBanned")]
	public bool IsBanned { get; set; }

	[JsonPropertyName("externalAppDisplayName")]
	public string ExternalAppDisplayName { get; set; }

	[JsonPropertyName("hasVerifiedBadge")]
	public bool HasVerifiedBadge { get; set; }

	[JsonPropertyName("id")]
	public long Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("displayName")]
	public string DisplayName { get; set; }
}
