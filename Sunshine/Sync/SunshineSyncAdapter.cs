using System;
using Android.Content;
using Android.OS;
using Android.Accounts;
using System.Net.Http;
using ModernHttpClient;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using Sunshine.Data;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Graphics;
using Android.App;
using Android.Support.V4.Content;
using Android.Util;
using Sunshine.Models;

namespace Sunshine.Sync
{
    public class SunshineSyncAdapter : AbstractThreadedSyncAdapter
    {

        const string LogTag = "SunshineSyncAdapter";
        const string SourceContext = "MyNamespace.MyClass";

        private const string _tag = "SunshineSyncAdapter";


        // Interval at which to sync with the weather, in seconds.
        // 60 seconds (1 minute) * 180 = 3 hours
        public const int SyncInterval = 60 * 180;

        // public const int SyncInterval = 20;

        public const int SyncFlextime = SyncInterval / 3;
        const int WeatherNotificationId = 3004;


        readonly string[] _notifyWeatherProjection = new []
        {
            WeatherContract.Weather.ColumnWeatherId,
            WeatherContract.Weather.ColumnMaximumTemp,
            WeatherContract.Weather.ColumnMinimumTemp,
            WeatherContract.Weather.ColumnShortDescription
        };

        // these indices must match the projection
        const int IndexWeatherId = 0;
        const int IndexMaximumTemp = 1;
        const int IndexMinimumTemp = 2;
        const int IndexShortDesc = 3;

        public SunshineSyncAdapter(Context context, bool autoInitialize)
            : base(context, autoInitialize)
        {
         
        
        }

        void NotifyWeather()
        {
            Context context = Context;
            //checking the last update and notify if it' the first of the day
            var prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var displayNotificationsKey = context.GetString(Resource.String.pref_enable_notifications_key);
            bool displayNotifications = prefs.GetBoolean(displayNotificationsKey,
                                            bool.Parse(context.GetString(Resource.String.pref_enable_notifications_default)));


            if (displayNotifications)
            {

                var lastNotificationKey = context.GetString(Resource.String.pref_last_notification);
                long lastSync = prefs.GetLong(lastNotificationKey, 0);

                var startDate = DateTime.UtcNow.StartOfDay();
                var saveDate = new DateTime(lastSync).StartOfDay();
                var diffDate = startDate - saveDate;

                if (diffDate.Days >= 1)
                {
                    // Last sync was more than 1 day ago, let's send a notification with the weather.
                    var locationQuery = Utility.GetPreferredLocation(context);

                    var weatherUri = WeatherContract.Weather.BuildWeatherLocationWithDate(locationQuery, DateTime.UtcNow.Ticks);

                    // we'll query our contentProvider, as always
                    var cursor = context.ContentResolver.Query(weatherUri, _notifyWeatherProjection, null, null, null);

                    if (cursor.MoveToFirst())
                    {
                        int weatherId = cursor.GetInt(IndexWeatherId);
                        double high = cursor.GetDouble(IndexMaximumTemp);
                        double low = cursor.GetDouble(IndexMinimumTemp);
                        String desc = cursor.GetString(IndexShortDesc);

                        int iconId = Utility.GetIconResourceForWeatherCondition(weatherId);
                        var resources = context.Resources;
                        var largeIcon = BitmapFactory.DecodeResource(resources,
                                            Utility.GetArtResourceForWeatherCondition(weatherId));

                        var title = context.GetString(Resource.String.app_name);

                        // Define the text of the forecast.
                        var contentText = Java.Lang.String.Format(context.GetString(Resource.String.format_notification),
                                              desc,
                                              Utility.FormatTemperature(context, high),
                                              Utility.FormatTemperature(context, low));
                    


                        // NotificationCompatBuilder is a very convenient way to build backward-compatible
                        // notifications.  Just throw in some data.
                        var builder =
                            new NotificationCompat.Builder(Context)
                                .SetColor(ContextCompat.GetColor(context, Resource.Color.sunshine_light_blue))
                                .SetSmallIcon(iconId)
                                .SetLargeIcon(largeIcon)
                                .SetContentTitle(title)
                                .SetContentText(contentText);

                        // Make something interesting happen when the user clicks on the notification.
                        // In this case, opening the app is sufficient.
                        var resultIntent = new Intent(context, typeof(MainActivity));

                        // The stack builder object will contain an artificial back stack for the
                        // started Activity.
                        // This ensures that navigating backward from the Activity leads out of
                        // your application to the Home screen.
                        var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(context);
                        stackBuilder.AddNextIntent(resultIntent);
                        var resultPendingIntent =
                            stackBuilder.GetPendingIntent(
                                0, (int)PendingIntentFlags.UpdateCurrent

                            );
                        builder.SetContentIntent(resultPendingIntent);

                        var mNotificationManager =
                            (NotificationManager)Context.GetSystemService(Context.NotificationService);
                        // WEATHER_NOTIFICATION_ID allows you to update the notification later on.
                        mNotificationManager.Notify(WeatherNotificationId, builder.Build());


                        //refreshing last sync
                        var editor = prefs.Edit();
                        editor.PutLong(lastNotificationKey, DateTime.UtcNow.Ticks);
                        editor.Commit();

                    }
                    cursor.Close();
                }
            }
        }

        /// <summary>
        /// Helper method to schedule the sync adapter periodic execution
        /// </summary>
        public static void ConfigurePeriodicSync(Context context, int syncInterval, int flexTime)
        {
            var account = GetSyncAccount(context);
            var authority = context.GetString(Resource.String.content_authority);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                // we can enable inexact timers in our periodic sync
                var request = new SyncRequest.Builder().
                                        SyncPeriodic(syncInterval, flexTime).
                                        SetSyncAdapter(account, authority).
                    SetExtras(new Bundle()).Build();
                ContentResolver.RequestSync(request);
            }
            else
            {
                ContentResolver.AddPeriodicSync(account,
                    authority, new Bundle(), syncInterval);
            }
        }


        static void OnAccountCreated(Account newAccount, Context context)
        {

            // Since we've created an account
            SunshineSyncAdapter.ConfigurePeriodicSync(context, SyncInterval, SyncFlextime);

            // Without calling setSyncAutomatically, our periodic sync will not be enabled.
            ContentResolver.SetSyncAutomatically(newAccount, context.GetString(Resource.String.content_authority), true);

            // Finally, let's do a sync to get things started
            SyncImmediately(context);
        }

        public static void InitializeSyncAdapter(Context context)
        {
            GetSyncAccount(context);
        }


        public async override void OnPerformSync(Android.Accounts.Account account, Android.OS.Bundle extras, string authority, ContentProviderClient provider, SyncResult syncResult)
        {

            Log.WriteLine(LogPriority.Debug, _tag, "Starting sync");


            try
            {

                var locationQuery = Utility.GetPreferredLocation(Context);

                // Constants for URL
                const string Url = "api.openweathermap.org";
                const string Path = "data/2.5/forecast/daily";
                const string Scheme = "http";


                // Constants for parameters
                const string Format = "json";
                const string Units = "metric";
                const int NumDays = 14;
                const string AppId = "582900aa4d4687a5711f7704ae16611a";

                var builder = new UriBuilder();
                var client = new HttpClient(new NativeMessageHandler());

                builder.Host = Url;
                builder.Path = Path;
                builder.Scheme = Scheme;
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["mode"] = Format;
                query["units"] = Units;
                query["cnt"] = NumDays.ToString();
                query["appid"] = AppId;
                query["q"] = locationQuery;
                builder.Query = query.ToString();

                HttpResponseMessage response = await client.GetAsync(builder.Uri);

                var responseJson = await response.Content.ReadAsStringAsync();
                GetWeatherDataFromJson(responseJson, locationQuery);

            }
            catch (Java.Net.UnknownHostException e)
            {
                Log.WriteLine(LogPriority.Debug, _tag, $"Download failed with result {e.Message}");
            }
            catch (JsonSerializationException e)
            {
                Log.WriteLine(LogPriority.Debug, _tag, $"Json parser failed with error {e.Message}");
            }

            return;

        }


        /// <summary>
        /// Take the String representing the complete forecast in JSON Format and
        /// pull out the data we need to construct the Strings needed for the wireframes.
        /// Fortunately parsing is easy:  constructor takes the JSON string and converts it
        /// into an Object hierarchy for us.
        /// </summary>
        void GetWeatherDataFromJson(string forecastJsonStr, string locationSetting)
        {
            try
            {
                var jsonResult = JsonConvert.DeserializeObject<Weather>(forecastJsonStr);
                var city = jsonResult.City;
                var locationId = AddLocation(locationSetting, city.Name, city.Coord.Lat, city.Coord.Lon);

                // Insert the new weather information into the database
                var contentValuesList = new List<ContentValues>(jsonResult.WeatherDaysList.Count);

                var timeNow = DateTime.UtcNow;
                var startDate = timeNow.StartOfDay();

                for (int i = 0; i < jsonResult.WeatherDaysList.Count; i++)
                {

                    // Current time and time in days
                    var dateTime = startDate.AddDays(i).Ticks;

                    int humidity = jsonResult.WeatherDaysList[i].Humidity;
                    double pressure = jsonResult.WeatherDaysList[i].Pressure;
                    double windSpeed = jsonResult.WeatherDaysList[i].Speed;
                    double windDirection = jsonResult.WeatherDaysList[i].Deg;


                    // Description is in a child array called "weather", which is 1 element long.
                    // That element also contains a weather code.
                    var description = jsonResult.WeatherDaysList[i].Weather[0].Main;
                    int weatherId = jsonResult.WeatherDaysList[i].Weather[0].Id;

                    // Temperatures are in a child object called "temp".  Try not to name variables
                    // "temp" when working with temperature.  It confuses everybody.
                    var high = jsonResult.WeatherDaysList[i].Temp.Max;
                    var low = jsonResult.WeatherDaysList[i].Temp.Min;


                    var weatherValues = new ContentValues();

                    weatherValues.Put(WeatherContract.Weather.ColumnLocationKey, locationId);
                    weatherValues.Put(WeatherContract.Weather.ColumnDate, dateTime);
                    weatherValues.Put(WeatherContract.Weather.ColumnHumidity, humidity);
                    weatherValues.Put(WeatherContract.Weather.ColumnPressure, pressure);
                    weatherValues.Put(WeatherContract.Weather.ColumnWindSpeed, windSpeed);
                    weatherValues.Put(WeatherContract.Weather.ColumnDegrees, windDirection);
                    weatherValues.Put(WeatherContract.Weather.ColumnMaximumTemp, high);
                    weatherValues.Put(WeatherContract.Weather.ColumnMinimumTemp, low);
                    weatherValues.Put(WeatherContract.Weather.ColumnShortDescription, description);
                    weatherValues.Put(WeatherContract.Weather.ColumnWeatherId, weatherId);

                    contentValuesList.Add(weatherValues);

                }                

                int inserted = 0;

                // add to database
                if (contentValuesList.Count > 0)
                {
                    inserted = Context.ContentResolver.BulkInsert(WeatherContract.Weather.ContentUri, contentValuesList.ToArray());

                    var yesterday = DateTime.UtcNow.StartOfDay().AddDays(-1);
                    Context.ContentResolver.Delete(WeatherContract.Weather.ContentUri,
                        WeatherContract.Weather.ColumnDate + " <= ?",
                        new string[] { yesterday.Ticks.ToString() });

                    NotifyWeather();
                }
                Log.WriteLine(LogPriority.Debug, _tag, $"FetchWeatherTask Complete {inserted} Inserted");

            }
            catch (JsonException e)
            {
                Log.WriteLine(LogPriority.Error, _tag, e.Message);
            }

        }

        /// <summary>
        /// Helper method to handle insertion of a new location in the weather database.
        /// </summary>
        /// <returns>The row ID of the added location</returns>
        /// <param name="locationSetting">The location string used to request updates from the server.</param>
        /// <param name="cityName">Human-readable city name, e.g "Mountain View"</param>
        /// <param name="lat">Latitude of the city</param>
        /// <param name="lon">Longitude of the city</param>
        public long AddLocation(string locationSetting, string cityName, double lat, double lon)
        {
            long locationId;

            var locationCursor = Context.ContentResolver.Query(WeatherContract.Location.ContentUri, 
                                     new string[]{ WeatherContract.Location.Id  }, 
                                     WeatherContract.Location.TableName +
                                     "." + WeatherContract.Location.ColumnLocationSetting + " = ?", 
                                     new string[]  { locationSetting }, 
                                     null);


            if (locationCursor.MoveToFirst())
            {
                int locationIdIndex = locationCursor.GetColumnIndex(WeatherContract.Location.Id);
                locationId = locationCursor.GetLong(locationIdIndex);

            }
            else
            {
                var contentValues = new ContentValues();
                contentValues.Put(WeatherContract.Location.ColumnCityName, cityName);
                contentValues.Put(WeatherContract.Location.ColumnLocationSetting, locationSetting);
                contentValues.Put(WeatherContract.Location.ColumnCoordinationLat, lat);
                contentValues.Put(WeatherContract.Location.ColumnCoordinationLong, lon);
                var insertedUri = Context.ContentResolver.Insert(WeatherContract.Location.ContentUri, contentValues);

                locationId = ContentUris.ParseId(insertedUri);


            }
            locationCursor.Close();
            return locationId;
        }


        /// <summary>
        /// Helper method to have the sync adapter sync immediately
        /// </summary>
        /// <param name="context">The context used to access the account service</param>
        public static void SyncImmediately(Context context)
        {
            var bundle = new Bundle();
            bundle.PutBoolean(ContentResolver.SyncExtrasExpedited, true);
            bundle.PutBoolean(ContentResolver.SyncExtrasManual, true);
            ContentResolver.RequestSync(GetSyncAccount(context),
                context.GetString(Resource.String.content_authority), bundle);
        }

        /// <summary>
        /// Helper method to get the fake account to be used with SyncAdapter, or make a new one
        /// if the fake account doesn't exist yet.  If we make a new account, we call the
        /// onAccountCreated method so we can initialize things.
        /// </summary>
        /// <returns>A fake account.</returns>
        /// <param name="context">The context used to access the account service</param>
        public static Account GetSyncAccount(Context context)
        {
            // Get an instance of the Android account manager
            var accountManager =
                (AccountManager)context.GetSystemService(Context.AccountService);

            // Create the account type and default account
            var newAccount = new Account(
                                 context.GetString(Resource.String.app_name), context.GetString(Resource.String.sync_account_type));

            // If the password doesn't exist, the account doesn't exist
            if (accountManager.GetPassword(newAccount) == null)
            {

                // Add the account and account type, no password or user data
                // If successful, return the Account object, otherwise report an error.
                if (!accountManager.AddAccountExplicitly(newAccount, "", null))
                {
                    return null;
                }
                   
                OnAccountCreated(newAccount, context);

                // If you don't set android:syncable="true" in
                // in your <provider> element in the manifest,
                // then call ContentResolver.setIsSyncable(account, AUTHORITY, 1)
                //here.
            }
            return newAccount;
        }
    }
}

