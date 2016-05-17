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
using Sunshine.Sync;
using Android.Runtime;
using System;
using System.Threading.Tasks;
using Android.Content.Res;

namespace Sunshine
{
    [Activity(MainLauncher = true, Theme = "@style/ForecastTheme", Label = "@string/app_name", Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity, Sunshine.ForecastFragment.ICallback
    {

        const string DetailFragmentTag = "DFTAG";
        const string HockeyAppId = "b632892031c04a5cb7aecc6452a0b1e4";
        string _location;
        bool _twoPane;
        bool _isMetric;


        public void StartHockeyApp()
        {

       

            // Register the crash manager before Initializing the trace writer
            HockeyApp.CrashManager.Register(this, HockeyAppId); 

            // Initialize the Trace Writer
            HockeyApp.TraceWriter.Initialize();

            // Wire up Unhandled Expcetion handler from Android
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                // Use the trace writer to log exceptions so HockeyApp finds them
                HockeyApp.TraceWriter.WriteTrace(args.Exception);
                args.Handled = true;
            };

            // Wire up the .NET Unhandled Exception handler
            AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.ExceptionObject);

            // Wire up the unobserved task exception handler
            TaskScheduler.UnobservedTaskException += 
            (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.Exception);


        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            #if !DEBUG
                StartHockeyApp();
            #endif

            var res = ApplicationContext.Resources;


            var locale = new Java.Util.Locale("en");

            Java.Util.Locale.Default = locale;

            var config = new Configuration();
            config.Locale = locale;
            res.UpdateConfiguration(config, res.DisplayMetrics);

            SetContentView(Resource.Layout.activity_main);

            _location = Utility.GetPreferredLocation(this);
            _isMetric = Utility.IsMetric(this);


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
            }
            else
            {
                _twoPane = false;
                SupportActionBar.Elevation = 0f;
            }

            var forecastFragment = ((ForecastFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_forecast));
            forecastFragment.SetUseTodayLayout(!_twoPane);
            SunshineSyncAdapter.InitializeSyncAdapter(this);
        }


        public void OnItemSelected(Android.Net.Uri contentUri)
        {
            if (_twoPane)
            {
                // In two-pane mode, show the detail view in this activity by
                // adding or replacing the detail fragment using a
                // fragment transaction.
                var args = new Bundle();
                args.PutParcelable(DetailFragment.DetailUri, contentUri);

                var fragment = new DetailFragment();
                fragment.Arguments = args;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.weather_detail_container, fragment, DetailFragmentTag).Commit();
            }
            else
            {
                var intent = new Intent(this, typeof(DetailActivity));
                intent.SetData(contentUri);
                StartActivity(intent);
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
            var isMetric = Utility.IsMetric(this);

            // update the location in our second pane using the fragment manager
            if (location != null && !location.Equals(_location))
            {
                var forecastFragment = (ForecastFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_forecast);
                if (forecastFragment != null)
                {
                    forecastFragment.OnLocationChanged();
                }
                var detailFragment = (DetailFragment)SupportFragmentManager.FindFragmentByTag(DetailFragmentTag);
                if (detailFragment != null)
                {
                    detailFragment.OnLocationChanged(location);
                }

                _location = location;
            }
            else if (isMetric != _isMetric)
            {
                var forecastFragment = (ForecastFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_forecast);
                if (forecastFragment != null)
                {
                    forecastFragment.OnMetricChanged();
                }
                var detailFragment = (DetailFragment)SupportFragmentManager.FindFragmentByTag(DetailFragmentTag);
                if (detailFragment != null)
                {
                    detailFragment.OnMetricChanged();
                }

                _isMetric = isMetric;
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
            

                default:
                    return base.OnOptionsItemSelected(item);

            }
        }

      
       
    }
}
