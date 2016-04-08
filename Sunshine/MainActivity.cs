using Android.App;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using Android.Content;
using Serilog;
using Android.Preferences;
using Android.Net;
using Serilog.Core;
using Serilog.Events;

namespace Sunshine
{
    [Activity(MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {

        const string DetailFragmentTag = "DFTAG";
        string _location;
        private bool _twoPane;
        ILogger _log;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _location = Utility.GetPreferredLocation(this);

            // Logger instance
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();

            if (FindViewById(Resource.Id.weather_detail_container) != null)
            {

                // The detail container view will be present only in the large-screen layouts
                // (res/layout-sw600dp). If this view is present, then the activity should be
                // in two-pane mode.

               
                _twoPane = true;
                // In two-pane mode, show the detail view in this activity by
                // adding or replacing the detail fragment using a
                // fragment transaction.
                if (savedInstanceState == null)
                {
                    SupportFragmentManager.BeginTransaction()
                        .Replace(Resource.Id.weather_detail_container, new DetailFragment(), DetailFragmentTag)
                                            .Commit();
                    
                                    
                }
                else
                {
                    _twoPane = false;
                }
            }            

        }

        /// <summary>
        /// Raises the create options menu event.
        ///  /// <returns>To be added.</returns>
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Main, menu);
            return true;
        }


        protected override void OnResume()
        {
            base.OnResume();

            var location = Utility.GetPreferredLocation(this);
            // update the location in our second pane using the fragment manager
            if (location != null && !location.Equals(_location))
            {
                var forecastFragment = (ForecastFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_forecast);
                if (forecastFragment != null)
                {
                    forecastFragment.OnLocationChanged();
                }
                _location = location;
            }
        }

        /// <summary>
        /// Raises the options item selected event.
        /// </summary>
        /// <returns>To be added.</returns>
        /// <para tool="javadoc-to-mdoc">This hook is called whenever an item in your options menu is selected.
        ///  The default implementation simply returns false to have the normal
        ///  processing happen (calling the item's Runnable or sending a message to
        ///  its Handler as appropriate). You can use this method for any items
        ///  for which you would like to do processing without those other
        ///  facilities.</para>
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.
            int id = item.ItemId;
            switch (id)
            {
                case Resource.Id.action_settings:

                    var intent = new Intent(this, typeof(SettingsActivity));
                    StartActivity(intent);
                    return true;
            
                case Resource.Id.action_map:

                    OpenPreferredLocationOnMap();
                    return true;


                default:
                    return base.OnOptionsItemSelected(item);

            }
        }

        /// <summary>
        /// Opens the preferred location on map
        /// </summary>
        void OpenPreferredLocationOnMap()
        {
           
            string location = Utility.GetPreferredLocation(this);
           
            // Using the URI scheme for showing a location found on a map.  This super-handy
            // intent can is detailed in the "Common Intents" page of Android's developer site:
            // http://developer.android.com/guide/components/intents-common.html#Maps
            Uri geoLocation = Uri.Parse("geo:0,0?").BuildUpon()
                                .AppendQueryParameter("q", location)
                                .Build();
            
            var intent = new Intent(Android.Content.Intent.ActionView);
            intent.SetData(geoLocation);

            if (intent.ResolveActivity(PackageManager) != null)
            {
                StartActivity(intent);
            }
            else
            {
                _log.ForContext<ForecastFragment>().Error("Couldn't call {0}, no receiving apps installed!", location);
            }
        }
       
    }
}
