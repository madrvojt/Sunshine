using System;

using Android.OS;
using Android.Views;
using Android.Widget;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Sunshine.Data;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Database;
using Android.Content;

namespace Sunshine
{
    /// <summary>
    ///  A placeholder fragment containing a simple view.
    /// </summary>
    public class ForecastFragment : Android.Support.V4.App.Fragment, LoaderManager.ILoaderCallbacks
    {

        ForecastAdapter _forecastAdapter;
        const string SourceContext = "MyNamespace.MyClass";
        readonly ILogger _log;
        const int ForecastLoader = 0;


        /// <summary>
        /// A callback interface that all activities containing this fragment must
        /// implement. This mechanism allows activities to be notified of item
        /// selections
        /// </summary>
        public interface ICallback
        {
            // DetailFragmentCallback for when an item has been selected.
            void OnItemSelected(Android.Net.Uri dateUri);
        }

        static string[] forecastColumns =
            {
                // In this case the id needs to be fully qualified with a table name, since
                // the content provider joins the location & weather tables in the background
                // (both have an _id column)
                // On the one hand, that's annoying.  On the other, you can search the weather table
                // using the location set by the user, which is only in the Location table.
                // So the convenience is worth it.
                WeatherContract.Weather.TableName + "." + WeatherContract.Weather.Id,
                WeatherContract.Weather.ColumnDate,
                WeatherContract.Weather.ColumnShortDescription,
                WeatherContract.Weather.ColumnMaximumTemp,
                WeatherContract.Weather.ColumnMinimumTemp,
                WeatherContract.Location.ColumnLocationSetting,
                WeatherContract.Weather.ColumnWeatherId,
                WeatherContract.Location.ColumnCoordinationLat,
                WeatherContract.Location.ColumnCoordinationLong
            };


        // These indices are tied to FORECAST_COLUMNS.  If FORECAST_COLUMNS changes, these
        // must change. - This is mapping index for projection (in this case forecastcolumns)
        public const int ColWeatherId = 0;
        public const int ColWeatherDate = 1;
        public const int ColWeatherDesc = 2;
        public const int ColWeatherMaxTemp = 3;
        public const int ColWeatherMinTemp = 4;
        public const int ColLocationSettings = 5;
        public const int ColWeatherConditionId = 6;
        public const int ColCoordLat = 7;
        public const int ColCoordLon = 8;


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

        public Android.Support.V4.Content.Loader OnCreateLoader(int id, Bundle args)
        {
            string locationSetting = Utility.GetPreferredLocation(Activity);

            // Sort order:  Ascending, by date.
            const string sortOrder = WeatherContract.Weather.ColumnDate + " ASC";
            var weatherForLocationUri = WeatherContract.Weather.BuildWeatherLocationWithStartDate(
                                            locationSetting, DateTime.Now.Ticks);

            return new Android.Support.V4.Content.CursorLoader(Activity, weatherForLocationUri, forecastColumns, null, null, sortOrder);


        }

        public void OnLoadFinished(Android.Support.V4.Content.Loader loader, Java.Lang.Object data)
        {
            _forecastAdapter.SwapCursor((ICursor)data);
        }

        public void OnLoaderReset(Android.Support.V4.Content.Loader loader)
        {
            _forecastAdapter.SwapCursor(null);

        }


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            LoaderManager.InitLoader(ForecastLoader, null, this);

            base.OnActivityCreated(savedInstanceState);
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


        public void OnLocationChanged()
        {
            UpdateWeather();
            LoaderManager.RestartLoader(ForecastLoader, null, this);
        }

        async void UpdateWeather()
        {
                 
            var weatherTask = new FetchWeatherTask(Activity);
            var location = Utility.GetPreferredLocation(Activity);
            await weatherTask.GetDataFromServer(location);

        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {


            // The CursorAdapter will take data from our cursor and populate the ListView
            // However, we cannot use FLAG_AUTO_REQUERY since it is deprecated, so we will end
            // up with an empty list the first time we run.
            _forecastAdapter = new ForecastAdapter(Activity, null, 0);

            var rootView = inflater.Inflate(Resource.Layout.fragment_main, container, false);
            var listviewForecast = rootView.FindViewById<ListView>(Resource.Id.listview_forecast);

            // Reference to ListView
            listviewForecast.Adapter = _forecastAdapter;



            listviewForecast.ItemClick += (sender, e) =>
            {                   
                var cursor = (ICursor)((AdapterView)sender).GetItemAtPosition(e.Position);
                if (cursor != null)
                {
                    var locationSetting = Utility.GetPreferredLocation(Activity);
                    
                    ((ICallback)Activity).OnItemSelected(WeatherContract.Weather.BuildWeatherLocationWithDate(
                            locationSetting, cursor.GetLong(ColWeatherDate)));
                }
            };

            return rootView;
        }

       

    }
}

