using System;
using Newtonsoft.Json;

namespace Sunshine.JSONobject
{
    /// <summary>
    /// All temperatures are children of the "temp" object.
    /// </summary>
    public class Temperature
    {
        [JsonProperty(PropertyName = "max")]
        public double Max { get; set; }

        [JsonProperty(PropertyName = "min")]
        public double Min { get; set; }
   
    }
}

