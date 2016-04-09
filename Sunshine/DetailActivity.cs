using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using Android.App;
using Android.Content;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Widget;

namespace Sunshine
{
    [Activity(Label = "@string/title_activity_detail", Icon = "@mipmap/ic_launcher", ParentActivity = typeof(MainActivity))]            
    [MetaData("android.support.PARENT_ACTIVITY", Value = "cz.madrvojt.xamarin.sunshine.MainActivity")]
    public class DetailActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.ShareActionProvider _shareActionProvider;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_detail);

            var arguments = new Bundle();
            arguments.PutParcelable(DetailFragment.DetailUri, Intent.Data);

            var fragment = new DetailFragment();
            fragment.Arguments = arguments;



            if (savedInstanceState == null)
            {
                SupportFragmentManager.BeginTransaction()
                    .Add(Resource.Id.weather_detail_container, fragment)
                    .Commit();

            }
        }

        /// <summary>
        /// Raises the create options menu event.
        ///  /// <returns>To be added.</returns>
        /// </summary>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Detail, menu);

            return true;
        }


        // Call to update the share intent
        void SetShareIntent(Intent shareIntent)
        {
            if (_shareActionProvider != null)
            {
                _shareActionProvider.SetShareIntent(shareIntent);
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

