using System;
using Android.Content;

namespace Sunshine.Data
{
    public sealed class WeatherContract
    {
        // The "Content authority" is a name for the entire content provider, similar to the
        // relationship between a domain name and its website.  A convenient string to use for the
        // content authority is the package name for the app, which is guaranteed to be unique on the
        // device.
        public const string ContentAuthority = "com.example.android.sunshine.app";


        // Use CONTENT_AUTHORITY to create the base of all URI's which apps will use to contact
        // the content provider.
        public static Android.Net.Uri BaseContentUri = Android.Net.Uri.Parse($"content://{ContentAuthority}");

        // Possible paths (appended to base content URI for possible URI's)
        // For instance, content://com.example.android.sunshine.app/weather/ is a valid path for
        // looking at weather data. content://com.example.android.sunshine.app/givemeroot/ will fail,
        // as the ContentProvider hasn't been given any information on what to do with "givemeroot".
        // At least, let's hope not.  Don't be that dev, reader.  Don't be that dev.
        public const string PathWeather = "weather";
        public const string PathLocation = "location";

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

            public static Android.Net.Uri ContentUri =
                BaseContentUri.BuildUpon().AppendPath(PathLocation).Build();

            // Define Content type (media type) like classic MIME (First part is type, second is path for query)
            // Inform serdes
            // Content type for DIR
            public const string ContentType =
                ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "/" + PathLocation;
            // Content type for ITEM
            public const string ContentItemType =
                ContentResolver.CursorItemBaseType + "/" + ContentAuthority + "/" + PathLocation;

            // Build specific URI for Location
            public static Android.Net.Uri BuildLocationUri(long id)
            {
                return ContentUris.WithAppendedId(ContentUri, id);
            }
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


            public static Android.Net.Uri ContentUri =
                BaseContentUri.BuildUpon().AppendPath(PathWeather).Build();

            public const string ContentType =
                ContentResolver.CursorDirBaseType + "/" + ContentAuthority + "/" + PathWeather;

            public const string ContentItemType =
                ContentResolver.CursorItemBaseType + "/" + ContentAuthority + "/" + PathWeather;

            // Get specific URI for weather
            public static Android.Net.Uri BuildWeatherUri(long id)
            {
                return ContentUris.WithAppendedId(ContentUri, id);
            }

            /// <summary>
            /// Builds the weather location.
            /// </summary>
            /// <returns>The weather location.</returns>
            /// <description>
            /// Student: Fill in this buildWeatherLocation function
            /// </description>
            public static Android.Net.Uri BuildWeatherLocation(string locationSetting)
            {
                return ContentUri.BuildUpon().AppendPath(locationSetting).Build();
            }

            public static Android.Net.Uri BuildWeatherLocationWithStartDate(
                string locationSetting, long startDate)
            {
                long normalizedDate = NormalizeDate(startDate);
                return ContentUri.BuildUpon().AppendPath(locationSetting)
                    .AppendQueryParameter(ColumnDate, normalizedDate.ToString()).Build();
            }

            public static Android.Net.Uri BuildWeatherLocationWithDate(string locationSetting, long date)
            {
                return ContentUri.BuildUpon().AppendPath(locationSetting)
                    .AppendPath(NormalizeDate(date).ToString()).Build();
            }

            public static string GetLocationSettingFromUri(Android.Net.Uri uri)
            {
                return uri.PathSegments[1];
            }

            public static long GetDateFromUri(Android.Net.Uri uri)
            {
                return long.Parse(uri.PathSegments[2]);
            }

            public static long GetStartDateFromUri(Android.Net.Uri uri)
            {
                var dateString = uri.GetQueryParameter(ColumnDate);
                if (!string.IsNullOrEmpty(dateString))
                    return long.Parse(dateString);
                else
                    return 0;
            }



        }
    }
}

