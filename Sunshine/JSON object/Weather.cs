using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sunshine.JSONobject
{
    public class Weather
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get ; set; }

        [JsonProperty(PropertyName = "list")]
        public List<WeatherDaysList> WeatherDaysList { get ; set ; }

        [JsonProperty(PropertyName = "city")]
        public City City { get ; set ; }
    }
}

