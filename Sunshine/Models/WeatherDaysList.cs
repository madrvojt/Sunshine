using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sunshine.Models
{
    /// <summary>
    /// Weather information.  Each day's forecast info is an element of the "list" array.
    /// </summary>
    public class WeatherDaysList
    {
        [JsonProperty(PropertyName = "temp")]
        public Temperature Temp { get; set; }

        [JsonProperty(PropertyName = "weather")]
        public List<WeatherDetail> Weather { get; set; }

        [JsonProperty(PropertyName = "pressure")]
        public double Pressure { get; set; }

        [JsonProperty(PropertyName = "humidity")]
        public int Humidity { get; set; }

        [JsonProperty(PropertyName = "speed")]
        public double Speed { get; set; }

        [JsonProperty(PropertyName = "deg")]
        public double Deg { get; set; }

    }
}

