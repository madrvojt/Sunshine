using System;
using Android.OS;
using Android.Widget;
using Android.Content;
using Newtonsoft.Json;
using Android.Preferences;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Threading.Tasks;
using System.Net.Http;
using Sunshine.Data;
using Sunshine.JSONobject;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ModernHttpClient;
using Android.Database;

namespace Sunshine
{
    public class FetchWeatherTask
    {

        // private final String LOG_TAG = FetchWeatherTask.class.getSimpleName();

        private readonly Context _context;
        const string SourceContext = "MyNamespace.MyClass";
        readonly ILogger _log;

        public FetchWeatherTask(Context context)
        {
            _context = context;
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();
        }

        // private boolean DEBUG = true;

        /// <summary>
        /// The date/time conversion code is going to be moved outside the asynctask later,
        /// so for convenience we're breaking it out into its own method now.
        /// </summary>
        /// <returns>The readable date string.</returns>
        /// <param name="time">Time.</param>
        string GetReadableDateString(long time)
        {
            // Because the API returns a unix timestamp (measured in seconds),
            // it must be converted to milliseconds in order to be converted to valid date.

            var dateTime = new DateTime(time);
            return dateTime.ToString("D");
        }

        /// <summary>
        /// Prepare the weather high/lows for presentation.
        /// </summary>
        /// <returns>The parse temperature</returns>
        string FormatHighLows(double high, double low)
        {

            // Data is fetched in Celsius by default.
            // If user prefers to see in Fahrenheit, convert the values here.
            // We do this rather than fetching in Fahrenheit so that the user can
            // change this option without us having to re-fetch the data once
            // we start storing the values in a database.

            ISharedPreferences sharedPreferences =
                PreferenceManager.GetDefaultSharedPreferences(_context);

            string unitType = sharedPreferences.GetString(
                                  _context.GetString(Resource.String.pref_units_key),
                                  _context.GetString(Resource.String.pref_units_metric));

            if (unitType.Equals(_context.GetString(Resource.String.pref_units_imperial)))
            {
                high = (high * 1.8) + 32;
                low = (low * 1.8) + 32;
            }
            else if (!unitType.Equals(_context.GetString(Resource.String.pref_units_metric)))
            {
                _log.ForContext<ForecastFragment>().Debug("Unit type not found {0}", unitType);                  
            }

            // For presentation, assume the user doesn't care about tenths of a degree.
            var roundedHigh = (long)Math.Round(high, 0);
            var roundedLow = (long)Math.Round(low, 0);

            String highLowStr = roundedHigh + "/" + roundedLow;
            return highLowStr;
        }


        /// <summary>
        /// Helper method to handle insertion of a new location in the weather database.
        /// </summary>
        /// <returns>The row ID of the added location</returns>
        /// <param name="locationSetting">The location string used to request updates from the server.</param>
        /// <param name="cityName">Human-readable city name, e.g "Mountain View"</param>
        /// <param name="lat">Latitude of the city</param>
        /// <param name="lon">Longitude of the city</param>
        public long AddLocation(string locationSetting, string cityName, double lat, double lon)
        {
            long locationId;

            var locationCursor = _context.ContentResolver.Query(WeatherContract.Location.ContentUri, 
                                     new string[]{ WeatherContract.Location.Id  }, 
                                     WeatherContract.Location.TableName +
                                     "." + WeatherContract.Location.ColumnLocationSetting + " = ?", 
                                     new string[]  { locationSetting }, 
                                     null);


            if (locationCursor.MoveToFirst())
            {
                int locationIdIndex = locationCursor.GetColumnIndex(WeatherContract.Location.Id);
                locationId = locationCursor.GetLong(locationIdIndex);
            
            }
            else
            {
                var contentValues = new ContentValues();
                contentValues.Put(WeatherContract.Location.ColumnCityName, cityName);
                contentValues.Put(WeatherContract.Location.ColumnLocationSetting, locationSetting);
                contentValues.Put(WeatherContract.Location.ColumnCoordinationLat, lat);
                contentValues.Put(WeatherContract.Location.ColumnCoordinationLong, lon);
                var insertedUri = _context.ContentResolver.Insert(WeatherContract.Location.ContentUri, contentValues);

                locationId = ContentUris.ParseId(insertedUri);

            
            }
            locationCursor.Close();
            return locationId;
        }


        /// <summary>
        /// Converts the content values to UX format.
        /// </summary>
        /// <returns>The content values to UX format.</returns>
        /// <param name="contentValue">Content value.</param>
        /// <description>
        /// Students: This code will allow the FetchWeatherTask to continue to return the strings that
        /// the UX expects so that we can continue to test the application even once we begin using
        /// the database.
        /// </description>        
        string[] ConvertContentValuesToUXFormat(IEnumerable<ContentValues> contentValue)
        {
            // return strings to keep UI functional for now
            String[] resultStrs = new String[contentValue.Count()];
            for (int i = 0; i < contentValue.Count(); i++)
            {
                var weatherValues = contentValue.ElementAt(i);

                string highAndLow = FormatHighLows(
                                        weatherValues.GetAsDouble(WeatherContract.Weather.ColumnMaximumTemp),
                                        weatherValues.GetAsDouble(WeatherContract.Weather.ColumnMinimumTemp));

                resultStrs[i] = GetReadableDateString(
                    weatherValues.GetAsLong(WeatherContract.Weather.ColumnDate)) +
                " - " + weatherValues.GetAsString(WeatherContract.Weather.ColumnShortDescription) +
                " - " + highAndLow;
            }
            return resultStrs;
        }


        /// <summary>
        /// Take the String representing the complete forecast in JSON Format and
        /// pull out the data we need to construct the Strings needed for the wireframes.
        /// Fortunately parsing is easy:  constructor takes the JSON string and converts it
        /// into an Object hierarchy for us.
        /// </summary>
        string[] GetWeatherDataFromJson(string forecastJsonStr, string locationSetting)
        {
            try
            {
                var jsonResult = JsonConvert.DeserializeObject<Weather>(forecastJsonStr);
                var city = jsonResult.City;
                var locationId = AddLocation(locationSetting, city.Name, city.Coord.Lat, city.Coord.Lon);

                // Insert the new weather information into the database
                var contentValuesList = new List<ContentValues>(jsonResult.WeatherDaysList.Count);
                              
                var timeNow = DateTime.Now;


                for (int i = 0; i < jsonResult.WeatherDaysList.Count; i++)
                {

                    // Current time and time in days
                    var currentDate = timeNow.AddDays(i).Ticks;
                    // Normalize time to UTC
                    var normalizeUtcTime = WeatherContract.NormalizeDate(currentDate);


                    int humidity = jsonResult.WeatherDaysList[i].Humidity;
                    double pressure = jsonResult.WeatherDaysList[i].Pressure;
                    double windSpeed = jsonResult.WeatherDaysList[i].Speed;
                    double windDirection = jsonResult.WeatherDaysList[i].Deg;


                    // Description is in a child array called "weather", which is 1 element long.
                    // That element also contains a weather code.
                    var description = jsonResult.WeatherDaysList[i].Weather[0].Main;
                    int weatherId = jsonResult.WeatherDaysList[i].Weather[0].Id;

                    // Temperatures are in a child object called "temp".  Try not to name variables
                    // "temp" when working with temperature.  It confuses everybody.
                    var high = jsonResult.WeatherDaysList[i].Temp.Max;
                    var low = jsonResult.WeatherDaysList[i].Temp.Min;


                    var weatherValues = new ContentValues();

                    weatherValues.Put(WeatherContract.Weather.ColumnLocationKey, locationId);
                    weatherValues.Put(WeatherContract.Weather.ColumnDate, normalizeUtcTime);
                    weatherValues.Put(WeatherContract.Weather.ColumnHumidity, humidity);
                    weatherValues.Put(WeatherContract.Weather.ColumnPressure, pressure);
                    weatherValues.Put(WeatherContract.Weather.ColumnWindSpeed, windSpeed);
                    weatherValues.Put(WeatherContract.Weather.ColumnDegrees, windDirection);
                    weatherValues.Put(WeatherContract.Weather.ColumnMaximumTemp, high);
                    weatherValues.Put(WeatherContract.Weather.ColumnMinimumTemp, low);
                    weatherValues.Put(WeatherContract.Weather.ColumnShortDescription, description);
                    weatherValues.Put(WeatherContract.Weather.ColumnWeatherId, weatherId);

                    contentValuesList.Add(weatherValues);

                }                

                // add to database
                if (contentValuesList.Count > 0)
                {
                    _context.ContentResolver.BulkInsert(WeatherContract.Weather.ContentUri, contentValuesList.ToArray());
                }

                // Sort order:  Ascending, by date.
                const string sortOrder = WeatherContract.Weather.ColumnDate + " ASC";
                var weatherForLocationUri = WeatherContract.Weather.BuildWeatherLocationWithStartDate(locationSetting, timeNow.Ticks);


                var cursor = _context.ContentResolver.Query(weatherForLocationUri,
                                 null, null, null, sortOrder);
                
                contentValuesList = new List<ContentValues>(cursor.Count);
                            
                while (cursor.MoveToNext())
                {
                    var cv = new ContentValues();
                    DatabaseUtils.CursorRowToContentValues(cursor, cv);
                    contentValuesList.Add(cv);
                }

                _log.ForContext<ForecastFragment>().Debug($"FetchWeatherTask Complete {contentValuesList.Count} Inserted");

                string[] resultStrs = ConvertContentValuesToUXFormat(contentValuesList);
                return resultStrs;


            }
            catch (JsonException e)
            {
                _log.ForContext<ForecastFragment>().Error(e, SourceContext, null);

            
            }


            return null;

        }



        /// <summary>
        /// Gets the data from server
        /// </summary>
        /// <returns>string array with value</returns>
        /// <param name = "locationQuery"></param>
        /// <param name="postCode">Post code value</param>
        public Task<string[]> GetDataFromServer(string locationQuery)
        { 
            return Task.Run(async () =>
                {
                    string[] result = null;
                    // If there's no zip code, there's nothing to look up.  Verify size of params.
                    if (string.IsNullOrWhiteSpace(locationQuery))
                    {
                        return null;
                    }
                  
                    try
                    {
                        // Constants for URL
                        const string Url = "api.openweathermap.org";
                        const string Path = "data/2.5/forecast/daily";
                        const string Scheme = "http";
                        const string Content = "data/2.5/forecast/daily?q=25219&mode=json&units=metric&cnt=7&appid=582900aa4d4687a5711f7704ae16611a";


                        // Constants for parameters
                        const string Format = "json";
                        const string Units = "metric";
                        const int NumDays = 7;
                        const string AppId = "582900aa4d4687a5711f7704ae16611a";

                        var builder = new UriBuilder();
                        var client = new HttpClient(new NativeMessageHandler());

                        builder.Host = Url;
                        builder.Path = Path;
                        builder.Scheme = Scheme;
                        var query = HttpUtility.ParseQueryString(builder.Query);
                        query["mode"] = Format;
                        query["units"] = Units;
                        query["cnt"] = NumDays.ToString();
                        query["appid"] = AppId;
                        query["q"] = locationQuery;
                        builder.Query = query.ToString();

                        HttpResponseMessage response = await client.GetAsync(builder.Uri);

                        var responseJson = await response.Content.ReadAsStringAsync();
                        result = GetWeatherDataFromJson(responseJson, locationQuery);

                    }
                    catch (Java.Net.UnknownHostException e)
                    {
                        _log.ForContext<ForecastFragment>().Error("Download failed with result {0}", e.Message);

                    }
                    catch (JsonSerializationException e)
                    {
                        _log.ForContext<ForecastFragment>().Error("Json parser failed with error {0}", e.Message);

                    }

                    return result;
                });
        }


    }
}

