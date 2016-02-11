using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Android.Support.V4.View;
using Sunshine.Data;
using Android.Database;
using System;

namespace Sunshine
{
    public class DetailFragment : Android.Support.V4.App.Fragment, LoaderManager.ILoaderCallbacks
    {
        const string ForecastShareHashtag = " #SunshineApp";
        string _forecastString;
        Android.Support.V7.Widget.ShareActionProvider _shareActionProvider;

        const string SourceContext = "MyNamespace.MyClass";
        readonly ILogger _log;


        const int DetailLoader = 0;
       
        string[] _forecastColumns =
            {
                WeatherContract.Weather.TableName + "." + WeatherContract.Weather.Id,
                WeatherContract.Weather.ColumnDate,
                WeatherContract.Weather.ColumnShortDescription,
                WeatherContract.Weather.ColumnMaximumTemp,
                WeatherContract.Weather.ColumnMinimumTemp,
            };

        // these constants correspond to the projection defined above, and must change if the
        // projection changes
        const int ColWeatherId = 0;
        const int ColWeatherDate = 1;
        const int ColWeatherDesc = 2;
        const int ColWeatherMaxTemp = 3;
        const int ColWeatherMinTemp = 4;



        public DetailFragment()
        {
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            return inflater.Inflate(Resource.Layout.fragment_detail, container, false);
        }


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            LoaderManager.InitLoader(DetailLoader, null, this);
            base.OnActivityCreated(savedInstanceState);
        }

        public Android.Support.V4.Content.Loader OnCreateLoader(int id, Bundle args)
        {
           
            var intent = Activity.Intent;
            if (intent == null)
            {
                return null;
            }

            // Now create and return a CursorLoader that will take care of
            // creating a Cursor for the data being displayed.
            return new Android.Support.V4.Content.CursorLoader(
                Activity,
                intent.Data,
                _forecastColumns,
                null,
                null,
                null
            );
        }

        public void OnLoadFinished(Android.Support.V4.Content.Loader loader, Java.Lang.Object data)
        {

            var dataCursor = (ICursor)data;

            if (dataCursor == null & !dataCursor.MoveToFirst())
            {
                return;
            }

            var dateString = Utility.FormatDate(
                                 dataCursor.GetLong(ColWeatherDate));
            
            var weatherDescription =
                dataCursor.GetString(ColWeatherDesc);
            
            bool isMetric = Utility.IsMetric(Activity);

            var high = Utility.FormatTemperature(
                           dataCursor.GetDouble(ColWeatherMaxTemp), isMetric);
            
            var low = Utility.FormatTemperature(
                          dataCursor.GetDouble(ColWeatherMinTemp), isMetric);
            
            _forecastString = $"{dateString} - {weatherDescription} - {high}/{low}";

            var detailTextView = View.FindViewById<TextView>(Resource.Id.detail_text);

                       
            detailTextView.Text = _forecastString;

            // If onCreateOptionsMenu has already happened, we need to update the share intent now.
            if (_shareActionProvider != null)
            {
                _shareActionProvider.SetShareIntent(CreateShareForecastIntent());
            }
        }

        public void OnLoaderReset(Android.Support.V4.Content.Loader loader)
        {
            throw new System.NotImplementedException();
        }

        Intent CreateShareForecastIntent()
        {
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.AddFlags(ActivityFlags.ClearWhenTaskReset);
            shareIntent.SetType("text/plain");

            shareIntent.PutExtra(Intent.ExtraText,
                _forecastString + ForecastShareHashtag);
            return shareIntent;
        }


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.detailfragment, menu);

            // Locate MenuItem with ShareActionProvider
            var item = menu.FindItem(Resource.Id.action_share);

            // Fetch and store ShareActionProvider
            _shareActionProvider = (Android.Support.V7.Widget.ShareActionProvider)MenuItemCompat.GetActionProvider(item);
            // If onLoadFinished happens before this, we can go ahead and set the share intent now.
            // Attach an intent to this ShareActionProvider.  You can update this at any time,
            // like when the user selects a new piece of data they might like to share.
            if (_forecastString != null)
            {
                _shareActionProvider.SetShareIntent(CreateShareForecastIntent());
            }    
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            switch (id)
            {
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

    }
}

