using System;
using Newtonsoft.Json;

namespace Sunshine.Models
{
    public class WeatherDetail
    {
        [JsonProperty(PropertyName = "main")]
        public string Main { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }
}

