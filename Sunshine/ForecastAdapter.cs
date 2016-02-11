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
            
        // Data is fetched in Celsius by default.
        // If user prefers to see in Fahrenheit, convert the values here.
        // We do this rather than fetching in Fahrenheit so that the user can
        // change this option without us having to re-fetch the data once
        // we start storing the values in a database.
        /// <summary>
        /// Prepare the weather high/lows for presentation.
        /// </summary>
        string FormatHighLows(double high, double low)
        {
            bool isMetric = Utility.IsMetric(_context);
            String highLowStr = Utility.FormatTemperature(high, isMetric) + "/" + Utility.FormatTemperature(low, isMetric);
            return highLowStr;
        }


        /// <summary>
        /// Converts the cursor row to UX format.
        /// </summary>
        /// <returns>The cursor row to UX format.</returns>
        /// <param name="cursor">Cursor.</param>
        string ConvertCursorRowToUXFormat(ICursor cursor)
        {
            string highAndLow = FormatHighLows(
                                    cursor.GetDouble(ForecastFragment.ColWeatherMaxTemp),
                                    cursor.GetDouble(ForecastFragment.ColWeatherMinTemp));
            
            return Utility.FormatDate(cursor.GetLong(ForecastFragment.ColWeatherDate)) +
            " - " + cursor.GetString(ForecastFragment.ColWeatherDesc) +
            " - " + highAndLow;
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
            var textview = (TextView)view;
            textview.Text = ConvertCursorRowToUXFormat(cursor);
        }

    }
}

