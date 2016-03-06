using System;
using Android.Views;
using Android.Support.V4.Widget;
using Android.Content;
using Android.Database;
using Sunshine.Data;
using Android.Widget;

namespace Sunshine
{
    public class ForecastAdapter : Android.Support.V4.Widget.CursorAdapter
    {

        Context _context;

        public ForecastAdapter(Context context, ICursor c, int flags)
            : base(context, c, flags)
        {
            _context = context;
        }


        /// <summary>
        /// Remember that these views are reused as needed.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="cursor">Cursor.</param>
        /// <param name = "parent"></param>
        public override View NewView(Android.Content.Context context, ICursor cursor, ViewGroup parent)
        {
            View view = LayoutInflater.From(context).Inflate(Resource.Layout.list_item_forecast, parent, false);
            return view;
        }

        /// <summary>
        /// This is where we fill-in the views with the contents of the cursor.
        /// </summary>
        /// <param name="view">View.</param>
        /// <param name="context">Context.</param>
        /// <param name = "cursor"></param>
        public override void BindView(View view, Context context, ICursor cursor)
        {

            // our view is pretty simple here --- just a text view
            // we'll keep the UI functional with a simple (and slow!) binding.

            // Read weather icon ID from cursor
            int weatherId = cursor.GetInt(ForecastFragment.ColWeatherId);
            // Use placeholder image for now
            var iconView = view.FindViewById<ImageView>(Resource.Id.list_item_icon);
            iconView.SetImageResource(Resource.Drawable.ic_launcher);

            // TODO Read date from cursor
            var date = cursor.GetLong(ForecastFragment.ColWeatherDate);
            var dateView = view.FindViewById<TextView>(Resource.Id.list_item_date_textview);
            dateView.Text = Utility.GetFriendlyDayString(context, date);

            // TODO Read weather forecast from cursor
            var description = cursor.GetString(ForecastFragment.ColWeatherDesc);
            var descritiontView = view.FindViewById<TextView>(Resource.Id.list_item_forecast_textview);
            descritiontView.Text = description;

            // Read user preference for metric or imperial temperature units
            bool isMetric = Utility.IsMetric(context);

            // Read high temperature from cursor
            double high = cursor.GetDouble(ForecastFragment.ColWeatherMaxTemp);
            var highTempView = view.FindViewById<TextView>(Resource.Id.list_item_high_textview);

            highTempView.Text = Utility.FormatTemperature(high, isMetric);

            // TODO Read low temperature from cursor
            double low = cursor.GetDouble(ForecastFragment.ColWeatherMinTemp);
            var lowTempView = view.FindViewById<TextView>(Resource.Id.list_item_low_textview);

            lowTempView.Text = Utility.FormatTemperature(low, isMetric);


        }

    }
}

