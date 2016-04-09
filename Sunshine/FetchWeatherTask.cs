using System;
using Android.Content;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Threading.Tasks;
using System.Net.Http;
using Sunshine.Data;
using Sunshine.JSONobject;
using System.Collections.Generic;
using System.Web;
using ModernHttpClient;
using Android.Database;

namespace Sunshine
{
    public class FetchWeatherTask
    {

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
        /// Take the String representing the complete forecast in JSON Format and
        /// pull out the data we need to construct the Strings needed for the wireframes.
        /// Fortunately parsing is easy:  constructor takes the JSON string and converts it
        /// into an Object hierarchy for us.
        /// </summary>
        void GetWeatherDataFromJson(string forecastJsonStr, string locationSetting)
        {
            try
            {
                var jsonResult = JsonConvert.DeserializeObject<Weather>(forecastJsonStr);
                var city = jsonResult.City;
                var locationId = AddLocation(locationSetting, city.Name, city.Coord.Lat, city.Coord.Lon);

                // Insert the new weather information into the database
                var contentValuesList = new List<ContentValues>(jsonResult.WeatherDaysList.Count);
                              
                var timeNow = DateTime.UtcNow;
                var startDate = timeNow.StartOfDay();

                for (int i = 0; i < jsonResult.WeatherDaysList.Count; i++)
                {

                    // Current time and time in days
                    var dateTime = startDate.AddDays(i).Ticks;

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
                    weatherValues.Put(WeatherContract.Weather.ColumnDate, dateTime);
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

                int inserted = 0;

                // add to database
                if (contentValuesList.Count > 0)
                {
                    inserted = _context.ContentResolver.BulkInsert(WeatherContract.Weather.ContentUri, contentValuesList.ToArray());
                }

                _log.ForContext<ForecastFragment>().Debug($"FetchWeatherTask Complete {inserted} Inserted");


            }
            catch (JsonException e)
            {
                _log.ForContext<ForecastFragment>().Error(e, SourceContext, null);
            }

        }



        /// <summary>
        /// Gets the data from server
        /// </summary>
        /// <returns>string array with value</returns>
        /// <param name = "locationQuery"></param>
        /// <param name="postCode">Post code value</param>
        public Task GetDataFromServer(string locationQuery)
        { 
            return Task.Run(async () =>
                {
                    // If there's no zip code, there's nothing to look up.  Verify size of params.
                    if (string.IsNullOrWhiteSpace(locationQuery))
                    {
                        return Task.FromResult(-1);
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
                        GetWeatherDataFromJson(responseJson, locationQuery);

                    }
                    catch (Java.Net.UnknownHostException e)
                    {
                        _log.ForContext<ForecastFragment>().Error("Download failed with result {0}", e.Message);

                    }
                    catch (JsonSerializationException e)
                    {
                        _log.ForContext<ForecastFragment>().Error("Json parser failed with error {0}", e.Message);

                    }

                    return Task.FromResult(0);
                });
        }


    }
}

