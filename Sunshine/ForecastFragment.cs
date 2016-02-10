using System;

using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using ModernHttpClient;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Sunshine.JSONobject;
using Android.Preferences;
using System.Collections.Generic;

namespace Sunshine
{
    /// <summary>
    ///  A placeholder fragment containing a simple view.
    /// </summary>
    public class ForecastFragment : Android.Support.V4.App.Fragment
    {

        ArrayAdapter<string> _forecastAdapter;
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


        public override void OnStart()
        {
            base.OnStart();
            UpdateWeather();
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

                    UpdateWeather();                            
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void UpdateWeather()
        {

            var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Activity);
            var location = sharedPreferences.GetString(GetString(Resource.String.pref_location_key), GetString(Resource.String.pref_location_default));
            var weatherTask = new FetchWeatherTask(Activity);
            weatherTask.GetDataFromServer(location).ContinueWith(t =>
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
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_main, container, false);
            var listviewForecast = rootView.FindViewById<ListView>(Resource.Id.listview_forecast);

            _forecastAdapter = new ArrayAdapter<string>(
                // Current context
                Activity,
                // ID of list axml
                Resource.Layout.list_item_forecast,
                // ID of textView
                Resource.Id.list_item_forecast_textview,
                // Forecast Data
                new List<string>());

            // Reference to ListView
            listviewForecast.Adapter = _forecastAdapter;
            listviewForecast.ItemClick += (sender, e) =>
            {                   
                var intent = new Intent(Activity, typeof(DetailActivity));
                string forecast = _forecastAdapter.GetItem(e.Position);
                intent.PutExtra(Intent.ExtraText, forecast);
                StartActivity(intent);
            };

            return rootView;
        }

       

    }
}

