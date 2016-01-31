using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Android.Support.V4.View;

namespace Sunshine
{
    public class DetailFragment : Fragment
    {
        const string ForecastShareHashtag = " #SunshineApp";
        string _forecastString;
        const string SourceContext = "MyNamespace.MyClass";
        readonly ILogger _log;

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
            View rootView = inflater.Inflate(Resource.Layout.fragment_detail, container, false);
            var textView = rootView.FindViewById<TextView>(Resource.Id.detail_text);

            var intent = Activity.Intent;
            if (intent != null & intent.HasExtra(Android.Content.Intent.ExtraText))
            {
                _forecastString = intent.GetStringExtra(Android.Content.Intent.ExtraText);
                textView.Text = _forecastString;
            }

            return rootView;
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
            var shareActionProvider = (Android.Support.V7.Widget.ShareActionProvider)MenuItemCompat.GetActionProvider(item);

            // Attach an intent to this ShareActionProvider.  You can update this at any time,
            // like when the user selects a new piece of data they might like to share.
            if (shareActionProvider != null)
            {
                shareActionProvider.SetShareIntent(CreateShareForecastIntent());
            }
            else
            {
                _log.ForContext<ForecastFragment>().Error("Share Action Provider is null?");
                   
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

