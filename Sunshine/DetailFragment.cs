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
using Sunshine.Sync;
using Android.Runtime;

namespace Sunshine
{
    public class DetailFragment : Android.Support.V4.App.Fragment, LoaderManager.ILoaderCallbacks
    {
        const string ForecastShareHashtag = " #SunshineApp";
        string _forecastString;
        Android.Support.V7.Widget.ShareActionProvider _shareActionProvider;

        public const string DetailUri = "Uri";
        Android.Net.Uri _uri;


        const int DetailLoader = 0;
       
        string[] _forecastColumns =
            {
                WeatherContract.Weather.TableName + "." + WeatherContract.Weather.Id,
                WeatherContract.Weather.ColumnDate,
                WeatherContract.Weather.ColumnShortDescription,
                WeatherContract.Weather.ColumnMaximumTemp,
                WeatherContract.Weather.ColumnMinimumTemp,
                WeatherContract.Weather.ColumnHumidity,
                WeatherContract.Weather.ColumnPressure,
                WeatherContract.Weather.ColumnWindSpeed,
                WeatherContract.Weather.ColumnDegrees,
                WeatherContract.Weather.ColumnWeatherId,

                // This works because the WeatherProvider returns location data joined with
                // weather data, even though they're stored in two different tables.
                WeatherContract.Location.ColumnLocationSetting

            };

        // these constants correspond to the projection defined above, and must change if the
        // projection changes
        const int ColWeatherId = 0;
        const int ColWeatherDate = 1;
        const int ColWeatherDesc = 2;
        const int ColWeatherMaxTemp = 3;
        const int ColWeatherMinTemp = 4;
        const int ColWeatherHuminidy = 5;
        const int ColWeatherPressure = 6;
        const int ColWeatherWindSpeed = 7;
        const int ColWeatherDegrees = 8;
        const int ColWeatherConditionId = 9;


        ImageView _iconView;
        TextView _friendlyDateView;
        TextView _dateView;
        TextView _descriptionView;
        TextView _highTempView;
        TextView _lowTempView;
        TextView _humidityView;
        TextView _windView;
        TextView _pressureView;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            var arguments = Arguments;
            if (arguments != null)
            {
                _uri = (Android.Net.Uri)arguments.GetParcelable(DetailFragment.DetailUri);
            }

            View rootView = inflater.Inflate(Resource.Layout.fragment_detail, container, false);
            _iconView = rootView.FindViewById<ImageView>(Resource.Id.detail_icon);
            _dateView = rootView.FindViewById<TextView>(Resource.Id.detail_date_textview);
            _friendlyDateView = rootView.FindViewById<TextView>(Resource.Id.detail_day_textview);
            _descriptionView = rootView.FindViewById<TextView>(Resource.Id.detail_forecast_textview);
            _highTempView = rootView.FindViewById<TextView>(Resource.Id.detail_high_textview);
            _lowTempView = rootView.FindViewById<TextView>(Resource.Id.detail_low_textview);
            _humidityView = rootView.FindViewById<TextView>(Resource.Id.detail_humidity_textview);
            _windView = rootView.FindViewById<TextView>(Resource.Id.detail_wind_textview);
            _pressureView = rootView.FindViewById<TextView>(Resource.Id.detail_pressure_textview);
            return rootView;
        }


        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            LoaderManager.InitLoader(DetailLoader, null, this);
            base.OnActivityCreated(savedInstanceState);
        }

        public void OnMetricChanged()
        {
            LoaderManager.RestartLoader(DetailLoader, null, this);
        }

        public void OnLocationChanged(string newLocation)
        {
            // replace the uri, since the location has changed
            Android.Net.Uri uri = _uri;
            if (uri != null)
            {
                long date = WeatherContract.Weather.GetDateFromUri(uri);
                var updatedUri = WeatherContract.Weather.BuildWeatherLocationWithDate(newLocation, date);
                _uri = updatedUri;
                LoaderManager.RestartLoader(DetailLoader, null, this);
            }
        }

      
        public Android.Support.V4.Content.Loader OnCreateLoader(int id, Bundle args)
        {
            if (_uri != null)
            {
                // Now create and return a CursorLoader that will take care of
                // creating a Cursor for the data being displayed.
                return new Android.Support.V4.Content.CursorLoader(
                    Activity,
                    _uri,
                    _forecastColumns,
                    null,
                    null,
                    null
                );
            }
            return null;

        }

        public void OnLoadFinished(Android.Support.V4.Content.Loader loader, Java.Lang.Object data)
        {

            var dataCursor = data.JavaCast<ICursor>();
            if (dataCursor != null & dataCursor.MoveToFirst())
            {
         

                // Read weather condition ID from cursor
                int weatherId = dataCursor.GetInt(ColWeatherConditionId);
                // Use placeholder Image

                _iconView.SetImageResource(Utility.GetArtResourceForWeatherCondition(weatherId));


                // Read date from cursor and update views for day of week and date
                long date = dataCursor.GetLong(ColWeatherDate);
                var friendlyDateText = Utility.GetDayName(Activity, date);
                var dateText = Utility.GetFormattedMonthDay(Activity, date);
                _friendlyDateView.Text = friendlyDateText;
                _dateView.Text = dateText;



                // Read description from cursor and update view
                var description = dataCursor.GetString(ColWeatherDesc);
                _descriptionView.Text = description;

                // For accessibility, add a content description to the icon field
                _iconView.ContentDescription = description;

                double high = dataCursor.GetDouble(ColWeatherMaxTemp);
                var highString = Utility.FormatTemperature(Activity, high);
                _highTempView.Text = highString;


                // Read low temperature from cursor and update view
                var low = dataCursor.GetDouble(ColWeatherMinTemp);
                var lowString = Utility.FormatTemperature(Activity, low);
                _lowTempView.Text = lowString;

                // Read humidity from cursor and update view
                var humidity = dataCursor.GetDouble(ColWeatherHuminidy);
                _humidityView.Text = Activity.GetString(Resource.String.format_humidity, humidity);

                // Read wind speed and direction from cursor and update view
                var windSpeedStr = dataCursor.GetFloat(ColWeatherWindSpeed);
                var windDirStr = dataCursor.GetFloat(ColWeatherDegrees);

                _windView.Text = Utility.GetFormattedStrings(Activity, windSpeedStr, windDirStr);


                // Read pressure from cursor and update view
                var pressure = dataCursor.GetFloat(ColWeatherPressure);
                _pressureView.Text = Activity.GetString(Resource.String.format_pressure, pressure);

                // We still need this for the share intent
                _forecastString = Java.Lang.String.Format("%s - %s - %s/%s", dateText, description, high, low);


                // If onCreateOptionsMenu has already happened, we need to update the share intent now.
                if (_shareActionProvider != null)
                {
                    _shareActionProvider.SetShareIntent(CreateShareForecastIntent());
                }        
            }        
        }

        public void OnLoaderReset(Android.Support.V4.Content.Loader loader)
        {
            
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



    }
}

