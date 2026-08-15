using System.Text.Json.Serialization;

namespace Orbitstrap.Models.APIs.RoValra
{
    public class DatacenterLocation
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
