using Newtonsoft.Json;

namespace Sunshine.Models
{
    /// <summary>
    /// Location information
    /// </summary>
    public class City
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "coord")]
        public Coord Coord { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }


    }
}

