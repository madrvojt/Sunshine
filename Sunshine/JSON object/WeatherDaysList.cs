using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sunshine.JSONobject
{
    public class WeatherDaysList
    {
        [JsonProperty(PropertyName = "temp")]
        public Temperature Temp { get; set; }

        [JsonProperty(PropertyName = "weather")]
        public List<WeatherDetail> Weather { get; set; }
    }
}

