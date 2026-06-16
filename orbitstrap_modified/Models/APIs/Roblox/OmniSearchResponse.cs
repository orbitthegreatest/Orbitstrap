namespace Orbitstrap.Models.APIs.Roblox
{
    public class OmniSearchResponse
    {
        [JsonPropertyName("searchResults")]
        public List<OmniSearchGroup>? SearchResults { get; set; }
    }
}