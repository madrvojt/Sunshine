using System;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using ModernHttpClient;
using System.Net;
using System.Threading.Tasks;
using Serilog;
using AndroidLog = Android.Util.Log;
using Serilog.Events;
using Serilog.Core;
using System.Text;
using System.Web;

namespace Sunshine
{
    /// <summary>
    ///  A placeholder fragment containing a simple view.
    /// </summary>
    public class ForecastFragment : Android.Support.V4.App.Fragment
    {

        ArrayAdapter<string> _weatherAdapter;
        const string SourceContext = "MyNamespace.MyClass";
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
            
        public async override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            _log.ForContext<ForecastFragment>().Debug("test serilog");

            await GetDataFromServer("25219");
           
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

                    _log.ForContext<ForecastFragment>().Debug("Start action refresh");

                    GetDataFromServer("25219").ContinueWith(t =>
                        {

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

            var weekForecast = new List<string>();
            weekForecast.Add("Today - Sunny - 88 / 63");
            weekForecast.Add("Tomorrow - Foggy - 70 / 46");
            weekForecast.Add("Weds - Cloudy - 72 / 63");
            weekForecast.Add("Thurs - Rainy - 64 / 51");
            weekForecast.Add("Fri - Foggy - 70 / 46");
            weekForecast.Add("Sat - Sunny - 76 / 68");
            weekForecast.Add("Sun - Sunny - 76 / 68");

            _weatherAdapter = new ArrayAdapter<string>(
                // Current context
                Activity,
                // ID of list axml
                Resource.Layout.list_item_forecast,
                // ID of textView
                Resource.Id.list_item_forecast_textview,
                // Forecast Data
                weekForecast);

            // Reference to ListView
            var listviewForecast = rootView.FindViewById<ListView>(Resource.Id.listview_forecast);
            listviewForecast.Adapter = _weatherAdapter;
            return rootView;
        }
            
        Task<bool> GetDataFromServer(string postCode)
        { 
            return Task.Run(async () =>
                {
                    try
                    {
                        var client = new HttpClient(new NativeMessageHandler());


                        const string Url = "api.openweathermap.org";
                        const string Path = "data/2.5/forecast/daily";
                        const string Scheme =  "http";

                        const string Content = "data/2.5/forecast/daily?q=25219&mode=json&units=metric&cnt=7&appid=582900aa4d4687a5711f7704ae16611a";

                        var builder = new UriBuilder();

                        builder.Host = Url;
                        builder.Path = Path;
                        builder.Scheme = Scheme;
                        var query = HttpUtility.ParseQueryString(builder.Query);
                        query["mode"] = "json";
                        query["units"] = "metric";
                        query["cnt"] = "";
                        query["appid"] = "582900aa4d4687a5711f7704ae16611a";
                        query["q"] = postCode;
                        builder.Query = query.ToString();

                        _log.ForContext<ForecastFragment>().Verbose("Build Uri {0}", builder.Uri.ToString());

                        HttpResponseMessage response = await client.GetAsync(builder.Uri);

                    }
                    catch(Java.Net.UnknownHostException e)
                    {
                        _log.ForContext<ForecastFragment>().Error("Download failed with result {0}", e.Message);

                    }

                    return true;
                });
        }


    }
}

