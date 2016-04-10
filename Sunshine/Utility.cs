using System;
using Android.Content;
using Android.Preferences;
using Android.OS;
using Android.Support.V4.Content;

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


        public static DateTime StartOfDay(this DateTime theDate)
        {
            return theDate.Date;
        }


        public static string GetFormattedStrings(Context context, float windSpeed, float degrees)
        {
        
            int windFormat;

            if (Utility.IsMetric(context))
            {
                windFormat = Resource.String.format_wind_kmh;
            }
            else
            {
                windFormat = Resource.String.format_wind_mph;
                windSpeed = .621371192237334f * windSpeed;
            }


            string direction = "Unknown";
            if (degrees >= 337.5 || degrees < 22.5)
            {
                direction = "N";
            }
            else if (degrees >= 22.5 || degrees < 27.5)
            {
                direction = "NE";
            }
            else if (degrees >= 67.5 || degrees < 112.5)
            {
                direction = "E";
            }
            else if (degrees >= 112.5 || degrees < 157.5)
            {
                direction = "SE";
            }
            else if (degrees >= 157.5 || degrees < 202.5)
            {
                direction = "S";
            }
            else if (degrees >= 202.5 || degrees < 247.5)
            {
                direction = "SW";
            }
            else if (degrees >= 247.5 || degrees < 292.5)
            {
                direction = "W";
            }
            else if (degrees >= 292.5 || degrees < 22.5)
            {
                direction = "NW";
            }

            return Java.Lang.String.Format(context.GetString(windFormat), windSpeed, direction);
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
        public static string FormatTemperature(Context context, double temperature)
        {
            // Data stored in Celsius by default.  If user prefers to see in Fahrenheit, convert
            // the values here.
            if (!IsMetric(context))
            {
                temperature = (temperature * 1.8) + 32;
            }

            return Java.Lang.String.Format(context.GetString(Resource.String.format_temperature), temperature);
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
            var inputTime = new DateTime(dateInMillis);

            var timeDifference = inputTime - currentTime.Date;
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
                //return inputTime.ToString("ddd M");
                string dayName = GetDayName(context, dateInMillis);
                string smallDayName = dayName.Substring(0, 3);
                string name = inputTime.ToString("M");
                return smallDayName + " " + name;

            }
        }


        /// <summary>
        ///  Helper method to provide the icon resource id according to the weather condition id returned
        ///  by the OpenWeatherMap call. 
        /// </summary>  
        /// <param name="weatherId">Id from OpenWeatherMap API response</param>
        /// <returns>Resource id for the corresponding icon. -1 if no relation is found.</returns>
        public static int GetIconResourceForWeatherCondition(int weatherId)
        {
            // Based on weather code data found at:
            // http://bugs.openweathermap.org/projects/api/wiki/Weather_Condition_Codes
            if (weatherId >= 200 && weatherId <= 232)
            {
                return Resource.Drawable.ic_storm;
            }
            else if (weatherId >= 300 && weatherId <= 321)
            {
                return Resource.Drawable.ic_light_rain;
            }
            else if (weatherId >= 500 && weatherId <= 504)
            {
                return Resource.Drawable.ic_rain;
            }
            else if (weatherId == 511)
            {
                return Resource.Drawable.ic_snow;
            }
            else if (weatherId >= 520 && weatherId <= 531)
            {
                return Resource.Drawable.ic_rain;
            }
            else if (weatherId >= 600 && weatherId <= 622)
            {
                return Resource.Drawable.ic_snow;
            }
            else if (weatherId >= 701 && weatherId <= 761)
            {
                return Resource.Drawable.ic_fog;
            }
            else if (weatherId == 761 || weatherId == 781)
            {
                return Resource.Drawable.ic_storm;
            }
            else if (weatherId == 800)
            {
                return Resource.Drawable.ic_clear;
            }
            else if (weatherId == 801)
            {
                return Resource.Drawable.ic_light_clouds;
            }
            else if (weatherId >= 802 && weatherId <= 804)
            {
                return Resource.Drawable.ic_cloudy;
            }
            return -1;
        }

        /// <summary>
        /// Helper method to provide the art resource id according to the weather condition id returned
        /// </summary>
        /// <returns>Resource id for the corresponding icon. -1 if no relation is found.</returns>
        /// <param name="weatherId">Id from OpenWeatherMap API response</param>
        public static int GetArtResourceForWeatherCondition(int weatherId)
        {
            // Based on weather code data found at:
            // http://bugs.openweathermap.org/projects/api/wiki/Weather_Condition_Codes
            if (weatherId >= 200 && weatherId <= 232)
            {
                return Resource.Drawable.art_storm;
            }
            else if (weatherId >= 300 && weatherId <= 321)
            {
                return Resource.Drawable.art_light_rain;
            }
            else if (weatherId >= 500 && weatherId <= 504)
            {
                return Resource.Drawable.art_rain;
            }
            else if (weatherId == 511)
            {
                return Resource.Drawable.art_snow;
            }
            else if (weatherId >= 520 && weatherId <= 531)
            {
                return Resource.Drawable.art_rain;
            }
            else if (weatherId >= 600 && weatherId <= 622)
            {
                return Resource.Drawable.art_snow;
            }
            else if (weatherId >= 701 && weatherId <= 761)
            {
                return Resource.Drawable.art_fog;
            }
            else if (weatherId == 761 || weatherId == 781)
            {
                return Resource.Drawable.art_storm;
            }
            else if (weatherId == 800)
            {
                return Resource.Drawable.art_clear;
            }
            else if (weatherId == 801)
            {
                return Resource.Drawable.art_light_clouds;
            }
            else if (weatherId >= 802 && weatherId <= 804)
            {
                return Resource.Drawable.art_clouds;
            }
            return -1;
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
            var inputTime = new DateTime(dateInMillis);

            var timeDifference = inputTime - currentTime.Date;


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

