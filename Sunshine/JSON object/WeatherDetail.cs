using System;
using Newtonsoft.Json;

namespace Sunshine.JSONobject
{
    public class WeatherDetail
    {
        [JsonProperty(PropertyName = "main")]
        public string Main { get; set;}

//        [JsonProperty(PropertyName = "description")]
//        public string Description { get; set;}

    }
}

