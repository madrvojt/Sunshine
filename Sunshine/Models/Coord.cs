using System;
using Newtonsoft.Json;

namespace Sunshine.Models
{
    /// <summary>
    /// Location coordinate
    /// </summary>
    public class Coord
    {
        [JsonProperty(PropertyName = "lon")]
        public double Lon { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public double Lat { get; set; }
    }
}

