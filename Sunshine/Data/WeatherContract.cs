using System;

namespace Sunshine.Data
{
    public sealed class WeatherContract
    {
        /// <summary>
        /// Method for date normalize
        /// </summary>
        /// <description>
        /// To make it easy to query for the exact date, we normalize all dates that go into
        /// the database to the start of the the Julian day at UTC.
        /// </description>
        public static long NormalizeDate(long startDate)
        {
            // normalize the start date to the beginning of the (UTC) day                
            var time = new DateTime(startDate);
            return time.ToUniversalTime().Ticks;
        }

        /// <summary>
        /// Location entry for contract
        /// </summary>
        /// <description> 
        /// Inner class that defines the table contents of the location table
        /// Students: This is where you will add the strings.  (Similar to what has been
        /// done for WeatherEntry)
        /// </description> 
        public sealed class Location
        {
            public const string Id = "_id";
            public const string Count = "_count";
            public const string TableName = "location";
            public const string ColumnCityName = "city_name";
            public const string ColumnCoordinationLong = "coord_long";
            public const string ColumnCoordinationLat = "coord_lat";
            public const string ColumnLocationSetting = "location_settings";
        }

        /// <summary>
        /// Weather entry for contract
        /// </summary>
        /// <description>
        /// Inner class that defines the table contents of the weather table
        /// </description>
        public sealed class Weather
        {
            public const string Id = "_id";
            public const string Count = "_count";
            public const string TableName = "weather";

            // Column with the foreign key into the location table.
            public const string ColumnLocationKey = "location_id";
            // Date, stored as long in milliseconds since the epoch
            public const string ColumnDate = "date";
            // Weather id as returned by API, to identify the icon to be used
            public const string ColumnWeatherId = "weather_id";

            // Short description and long description of the weather, as provided by API.
            // e.g "clear" vs "sky is clear".
            public const string ColumnShortDescription = "short_desc";

            // Min and max temperatures for the day (stored as floats)
            public const string ColumnMinimumTemp = "min";
            public const string ColumnMaximumTemp = "max";

            // Humidity is stored as a float representing percentage
            public const string ColumnHumidity = "humidity";

            // Humidity is stored as a float representing percentage
            public const string ColumnPressure = "pressure";

            // Windspeed is stored as a float representing windspeed  mph
            public const string ColumnWindSpeed = "wind";

            // Degrees are meteorological degrees (e.g, 0 is north, 180 is south).  Stored as floats.
            public const string ColumnDegrees = "degrees";
        }
    }
}

