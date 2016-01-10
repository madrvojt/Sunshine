using System;

using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using ModernHttpClient;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using System.Web;
using Newtonsoft.Json;
using Sunshine.JSONobject;

namespace Sunshine
{
    /// <summary>
    ///  A placeholder fragment containing a simple view.
    /// </summary>
    public class ForecastFragment : Android.Support.V4.App.Fragment
    {

        ArrayAdapter<string> _forecastAdapter;
        const string SourceContext = "MyNamespace.MyClass";
        const string PostCode = "25219";
        readonly ILogger _log;

        public ForecastFragment()
        {
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // set this for handling menu events like onCreateOptionsMenu
            HasOptionsMenu = true;
        }


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.forecastfragment, menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            int id = item.ItemId;

            switch (id)
            {

                case Resource.Id.action_refresh:

                    GetDataFromServer(PostCode).ContinueWith(t =>
                        {
                            if (t.Result != null)
                            {
                                _forecastAdapter.Clear();
                                // Add items to code
                                foreach (string result in t.Result)
                                {
                                    _forecastAdapter.Add(result);
                                }
                            }

                        }, TaskScheduler.FromCurrentSynchronizationContext());
                            
                    return true;


                case Resource.Id.action_settings:

                    return true;


                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_main, container, false);
            var listviewForecast = rootView.FindViewById<ListView>(Resource.Id.listview_forecast);

            GetDataFromServer(PostCode).ContinueWith(t =>
                {
                    _forecastAdapter = new ArrayAdapter<string>(
                        // Current context
                        Activity,
                        // ID of list axml
                        Resource.Layout.list_item_forecast,
                        // ID of textView
                        Resource.Id.list_item_forecast_textview,
                        // Forecast Data
                        t.Result);

                    // Reference to ListView
                    listviewForecast.Adapter = _forecastAdapter;

                }, TaskScheduler.FromCurrentSynchronizationContext());
            

            return rootView;
        }


        /// <summary>
        /// Gets the data from server
        /// </summary>
        /// <returns>string array with value</returns>
        /// <param name="postCode">Post code value</param>
        Task<string[]> GetDataFromServer(string postCode)
        { 
            return Task.Run(async () =>
                {
                    string[] result = null;

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
                        query["q"] = postCode;
                        builder.Query = query.ToString();

                        HttpResponseMessage response = await client.GetAsync(builder.Uri);

                        var responseJson = await response.Content.ReadAsStringAsync();
                        result = GetWeatherDataFromJson(responseJson, NumDays);

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
            // For presentation, assume the user doesn't care about tenths of a degree.
            var roundedHigh = (long)Math.Round(high, 0);
            var roundedLow = (long)Math.Round(low, 0);

            String highLowStr = roundedHigh + "/" + roundedLow;
            return highLowStr;
        }


        /// <summary>
        /// Take the String representing the complete forecast in JSON Format and
        /// pull out the data we need to construct the Strings needed for the wireframes.
        /// Fortunately parsing is easy:  constructor takes the JSON string and converts it
        /// into an Object hierarchy for us.
        /// </summary>
        string[] GetWeatherDataFromJson(string forecastJsonStr, int numDays)
        {
            string day;
            string description;
            string highAndLow;

            var timeNow = DateTime.Now;

            var jsonResult = JsonConvert.DeserializeObject<Weather>(forecastJsonStr);
            var resultStrs = new string[numDays];
          
            for (int i = 0; i < jsonResult.WeatherDaysList.Count; i++)
            {

                var max = jsonResult.WeatherDaysList[i].Temp.Max;
                var min = jsonResult.WeatherDaysList[i].Temp.Min;
                highAndLow = FormatHighLows(max, min);

                var currentDate = timeNow.AddDays(i);
                day = GetReadableDateString(currentDate.Ticks);
                description = jsonResult.WeatherDaysList[i].Weather[0].Main;

                resultStrs[i] = day + " - " + description + " - " + highAndLow;
            }                
            return resultStrs;

        }



    }
}

