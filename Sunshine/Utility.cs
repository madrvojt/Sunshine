﻿using System;
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
        /// <param name = "context"></param>
        /// <param name="temperature">Temperature.</param>
        /// <param name="isMetric">If set to <c>true</c> is metric.</param>
        public static string FormatTemperature(Context context, double temperature, bool isMetric)
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
            return context.GetString(Resource.String.format_temperature, temp);
        }


        /// <summary>
        /// Helper method to convert the database representation of the date into something to display
        /// to users.  As classy and polished a user experience as "20140102" is, we can do better.
        /// </summary>
        /// <returns>A user-friendly representation of the date.</returns>
        /// <param name="context">Context to use for resource localization</param>
        /// <param name="dateInMillis">The date in milliseconds</param>
        public static string GetFriendlyDayString(Context context, long dateInMillis)
        {
            // The day string for forecast uses the following logic:
            // For today: "Today, June 8"
            // For tomorrow:  "Tomorrow"
            // For the next 5 days: "Wednesday" (just the day name)
            // For all days after that: "Mon Jun 8"

            var currentTime = DateTime.UtcNow;
            var inputTime = new DateTime(dateInMillis).ToUniversalTime();

            var timeDifference = inputTime - currentTime;

            // If the date we're building the String for is today's date, the format
            // is "Today, June 24"
            if (timeDifference.Days == 0)
            {
                String today = context.GetString(Resource.String.today);
                int formatId = Resource.String.format_full_friendly_date;


                return string.Format(context.GetString(
                        formatId,
                        today,
                        GetFormattedMonthDay(context, dateInMillis)));
            
            }
            else if (timeDifference.Days < 7)
            {
                // If the input date is less than a week in the future, just return the day name.
                return GetDayName(context, dateInMillis);
            }
            else
            {
                // Otherwise, use the form "Mon Jun 3"
                return inputTime.ToString("ddd M");

            }
        }



        /// <summary>
        /// Converts db date format to the format "Month day", e.g "June 24".
        /// </summary>
        /// <returns>The day in the form of a string formatted "December 6"</returns>
        /// <param name="context">Context to use for resource localization.</param>
        /// <param name="dateInMillis">The db formatted date string, expected to be of the form specified Utility.DATE_FORMAT</param>
        public static string GetFormattedMonthDay(Context context, long dateInMillis)
        {

            var dateTime = new DateTime(dateInMillis);

            var monthDayString = String.Format("{0:MMMM d}", dateTime);
            return monthDayString;
        }


        /// <summary>
        /// Given a day, returns just the name to use for that day.
        /// E.g "today", "tomorrow", "wednesday".
        /// </summary>
        /// <returns>The day name.</returns>
        /// <param name="context">Context to use for resource localization</param>
        /// <param name="dateInMillis">The date in milliseconds</param>
        public static string GetDayName(Context context, long dateInMillis)
        {
            // If the date is today, return the localized version of "Today" instead of the actual
            // day name.

            var currentTime = DateTime.UtcNow;
            var inputTime = new DateTime(dateInMillis).ToUniversalTime();

            var timeDifference = inputTime - currentTime;


            if (timeDifference.Days == 0)
            {
                return context.GetString(Resource.String.today);
            }
            else if (timeDifference.Days == 1)
            {
                return context.GetString(Resource.String.tomorrow);
            }
            else
            {
                // Otherwise, the format is just the day of the week (e.g "Wednesday".)
                return inputTime.ToString("dddd");
            }
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
