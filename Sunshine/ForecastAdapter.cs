using System;
using Android.Views;
using Android.Support.V4.Widget;
using Android.Content;
using Android.Database;
using Sunshine.Data;
using Android.Widget;

namespace Sunshine
{
          
    /// <summary>
    /// View holder.
    /// </summary>
    /// <description>
    /// Remember that these views are reused as needed.Cache of the children views for a forecast list item.
    /// </description>
    public class ViewHolder : Java.Lang.Object
    {
        public readonly ImageView IconView;
        public readonly TextView DateView;
        public readonly TextView DescriptionView;
        public readonly TextView HighTempView;
        public readonly TextView LowTempView;

        public ViewHolder(View view)
        {
            IconView = view.FindViewById<ImageView>(Resource.Id.list_item_icon);
            DateView = view.FindViewById<TextView>(Resource.Id.list_item_date_textview);
            DescriptionView = view.FindViewById<TextView>(Resource.Id.list_item_forecast_textview);
            HighTempView = view.FindViewById<TextView>(Resource.Id.list_item_high_textview);
            LowTempView = view.FindViewById<TextView>(Resource.Id.list_item_low_textview);
        }
    }








    public class ForecastAdapter : Android.Support.V4.Widget.CursorAdapter
    {

        Context _context;
        const int ViewTypeToday = 0;
        const int ViewTypeFutureDay = 1;
        const int ViewTypeCountConst = 2;
        // Flag to determine if we want to use a separate view for "today".
        bool _useTodayLayout = true;


        public bool UseTodayLayout { set { _useTodayLayout = value; } }



        public ForecastAdapter(Context context, ICursor c, int flags)
            : base(context, c, flags)
        {
            _context = context;
        }


        public override int ViewTypeCount
        {
            get
            {
                return ViewTypeCountConst;
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0 && _useTodayLayout)
                return ViewTypeToday;
            else
                return ViewTypeFutureDay;
        }


        /// <summary>
        /// Remember that these views are reused as needed.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="cursor">Cursor.</param>
        /// <param name = "parent"></param>
        public override View NewView(Android.Content.Context context, ICursor cursor, ViewGroup parent)
        {
            var viewType = GetItemViewType(cursor.Position);
            int layoutId = -1;

            switch (viewType)
            {
                case ViewTypeToday:
                    {
                        layoutId = Resource.Layout.list_item_forecast_today;
                        break;
                    }
                case ViewTypeFutureDay:
                    {
                        layoutId = Resource.Layout.list_item_forecast;
                        break;
                    }
            }


            var view = LayoutInflater.From(context).Inflate(layoutId, parent, false);

            var viewHolder = new ViewHolder(view);
            view.Tag = viewHolder;

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
            var viewHolder = (ViewHolder)view.Tag;

            // Read weather icon ID from cursor
            int weatherId = cursor.GetInt(ForecastFragment.ColWeatherId);
            int viewType = GetItemViewType(cursor.Position);
            switch (viewType)
            {
                case ViewTypeToday:
                    {
                        // Get weather icon
                        viewHolder.IconView.SetImageResource(Utility.GetArtResourceForWeatherCondition(
                                cursor.GetInt(ForecastFragment.ColWeatherConditionId)));
                        break;
                    }
                case ViewTypeFutureDay:
                    {
                        // Get weather icon
                        viewHolder.IconView.SetImageResource(Utility.GetIconResourceForWeatherCondition(
                                cursor.GetInt(ForecastFragment.ColWeatherConditionId)));
                        break;
                    }
            }

            // TODO Read date from cursor
            var date = cursor.GetLong(ForecastFragment.ColWeatherDate);
            viewHolder.DateView.Text = Utility.GetFriendlyDayString(context, date);

            // TODO Read weather forecast from cursor
            var description = cursor.GetString(ForecastFragment.ColWeatherDesc);
            viewHolder.DescriptionView.Text = description;

            // Read user preference for metric or imperial temperature units
            bool isMetric = Utility.IsMetric(context);

            // Read high temperature from cursor
            double high = cursor.GetDouble(ForecastFragment.ColWeatherMaxTemp);

            viewHolder.HighTempView.Text = Utility.FormatTemperature(context, high, isMetric);

            // TODO Read low temperature from cursor
            double low = cursor.GetDouble(ForecastFragment.ColWeatherMinTemp);
            viewHolder.LowTempView.Text = Utility.FormatTemperature(context, low, isMetric);

        }

    }
}

