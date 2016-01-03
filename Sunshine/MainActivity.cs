using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Support.V7.App;
using System.Collections.Generic;

namespace Sunshine
{
    [Activity(MainLauncher = true, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            if (savedInstanceState == null)
            {
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.container, new PlaceholderFragment()).Commit();
            
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
            return id == Resource.Id.action_settings || base.OnOptionsItemSelected(item);

        }

       
        /// <summary>
        ///  A placeholder fragment containing a simple view.
        /// </summary>
        public class PlaceholderFragment : Android.Support.V4.App.Fragment
        {

            ArrayAdapter<string> _weatherAdapter;


            public PlaceholderFragment()
            {
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
        }
    }
}
