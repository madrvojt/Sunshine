using System;
using Android.Content;
using Android.Preferences;

namespace Sunshine
{
    public static class Utility
    {
        /// <summary>
        /// Gets the preferred location.
        /// </summary>
        /// <returns>The preferred location.</returns>
        /// <param name="context">Context.</param>
        public static string GetPreferredLocation(Context context)
        {
            var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            return sharedPreferences.GetString(context.GetString(Resource.String.pref_location_key), context.GetString(Resource.String.pref_location_default));

        }

        /// <summary>
        /// Determines if is metric the specified context.
        /// </summary>
        /// <returns><c>true</c> if is metric the specified context; otherwise, <c>false</c>.</returns>
        /// <param name="context">Context.</param>
        public static bool IsMetric(Context context)
        {

            var sharedPreferences =
                PreferenceManager.GetDefaultSharedPreferences(context);

            return sharedPreferences.GetString(
                context.GetString(Resource.String.pref_units_key),
                context.GetString(Resource.String.pref_units_metric))
                    .Equals(context.GetString(Resource.String.pref_units_metric));

        }

        /// <summary>
        /// Formats the temperature.
        /// </summary>
        /// <returns>The temperature.</returns>
        /// <param name="temperature">Temperature.</param>
        /// <param name="isMetric">If set to <c>true</c> is metric.</param>
        public static string FormatTemperature(double temperature, bool isMetric)
        {
            double temp;
            if (!isMetric)
            {
                temp = 9 * temperature / 5 + 32;
            }
            else
            {
                temp = temperature;
            }
            return temp.ToString("0");
        }



        public static string FormatDate(long dateInMillis)
        {
            // Because the API returns a unix timestamp (measured in seconds),
            // it must be converted to milliseconds in order to be converted to valid date.

            var dateTime = new DateTime(dateInMillis);
            return dateTime.ToString("D");
        }


    }
}

