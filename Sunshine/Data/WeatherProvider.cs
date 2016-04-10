using System;
using Android.Content;
using Android.Database.Sqlite;
using Android.Database;
using Android.Annotation;
using Serilog.Core;
using Serilog.Events;
using Serilog;

namespace Sunshine.Data
{
    [ContentProvider(new string[] { WeatherContract.ContentAuthority }, Exported = false, Syncable = true)]
    public class WeatherProvider : ContentProvider
    {
        readonly ILogger _log;

        public WeatherProvider()
        {
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();

        }


        // The URI Matcher used by this content provider.
        readonly UriMatcher _uriMatcher = BuildUriMatcher();
        WeatherDbHelper _openHelper;

        public const int Weather = 100;
        public const int WeatherWithLocation = 101;
        public const int WeatherWithLocationAndDate = 102;
        public const int Location = 300;

        SQLiteQueryBuilder _weatherByLocationSettingQueryBuilder;

        //location.location_setting = ?
        const string LocationSettingSelection =
            WeatherContract.Location.TableName +
            "." + WeatherContract.Location.ColumnLocationSetting + " = ? ";

        //location.location_setting = ? AND date >= ?
        const string LocationSettingWithStartDateSelection =
            WeatherContract.Location.TableName +
            "." + WeatherContract.Location.ColumnLocationSetting + " = ? AND " +
            WeatherContract.Weather.ColumnDate + " >= ? ";

        //location.location_setting = ? AND date = ?
        const string LocationSettingAndDaySelection =
            WeatherContract.Location.TableName +
            "." + WeatherContract.Location.ColumnLocationSetting + " = ? AND " +
            WeatherContract.Weather.ColumnDate + " = ? ";

        private ICursor GetWeatherByLocationSetting(Android.Net.Uri uri, string[] projection, string sortOrder)
        {
            string locationSetting = WeatherContract.Weather.GetLocationSettingFromUri(uri);
            long startDate = WeatherContract.Weather.GetStartDateFromUri(uri);

            string[] selectionArgs;
            string selection;

            if (startDate == 0)
            {
                selection = LocationSettingSelection;
                selectionArgs = new string[]{ locationSetting };
            }
            else
            {
                selectionArgs = new string[]{ locationSetting, startDate.ToString() };
                selection = LocationSettingWithStartDateSelection;
            }

            return _weatherByLocationSettingQueryBuilder.Query(_openHelper.ReadableDatabase,
                projection,
                selection,
                selectionArgs,
                null,
                null,
                sortOrder
            );
        }

        ICursor GetWeatherByLocationSettingAndDate(
            Android.Net.Uri uri, string[] projection, string sortOrder)
        {
            string locationSetting = WeatherContract.Weather.GetLocationSettingFromUri(uri);
            long date = WeatherContract.Weather.GetDateFromUri(uri);

            return _weatherByLocationSettingQueryBuilder.Query(_openHelper.ReadableDatabase,
                projection,
                LocationSettingAndDaySelection,
                new string[]{ locationSetting, date.ToString() },
                null,
                null,
                sortOrder
            );
        }

        /// <summary>
        /// Builds the URI matcher.
        /// </summary>
        /// <returns>The URI matcher.</returns>
        public static UriMatcher BuildUriMatcher()
        {
            var uriMatcher = new UriMatcher(UriMatcher.NoMatch);

            uriMatcher.AddURI(WeatherContract.ContentAuthority, WeatherContract.PathWeather, Weather);
            uriMatcher.AddURI(WeatherContract.ContentAuthority, WeatherContract.PathWeather + "/*", WeatherWithLocation);
            uriMatcher.AddURI(WeatherContract.ContentAuthority, WeatherContract.PathWeather + "/*/#", WeatherWithLocationAndDate);
            uriMatcher.AddURI(WeatherContract.ContentAuthority, WeatherContract.PathLocation, Location);
            return uriMatcher;
        }

        /// <Docs>Implement this to initialize your content provider on startup.</Docs>
        /// <remarks>Implement this to initialize your content provider on startup.
        ///  This method is called for all registered content providers on the
        ///  application main thread at application launch time. It must not perform
        ///  lengthy operations, or application startup will be delayed.</remarks>
        /// <summary>
        /// Raises the create event.
        /// </summary>
        /// <description>
        /// Students: We've coded this for you.  We just create a new WeatherDbHelper for later use
        /// here.
        /// </description>
        public override bool OnCreate()
        {
            _weatherByLocationSettingQueryBuilder = new SQLiteQueryBuilder();

            //This is an inner join which looks like
            //weather INNER JOIN location ON weather.location_id = location._id
            _weatherByLocationSettingQueryBuilder.Tables = 
                WeatherContract.Weather.TableName + " INNER JOIN " +
            WeatherContract.Location.TableName +
            " ON " + WeatherContract.Weather.TableName +
            "." + WeatherContract.Weather.ColumnLocationKey +
            " = " + WeatherContract.Location.TableName +
            "." + WeatherContract.Location.Id;


            _openHelper = new WeatherDbHelper(Context);
            return true;
        }

        /// <Docs>the URI to query.</Docs>
        /// <returns>To be added.</returns>
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <description>
        /// Students: Here's where you'll code the getType function that uses the UriMatcher.  You can
        /// test this by uncommenting testGetType in TestProvider.
        /// </description>
        public override string GetType(Android.Net.Uri uri)
        {
            // Use the Uri Matcher to determine what kind of URI this is.
            int match = _uriMatcher.Match(uri);

            switch (match)
            {
                case Weather:
                    return WeatherContract.Weather.ContentType;
                case Location:
                    return WeatherContract.Location.ContentType;

                case WeatherWithLocationAndDate:
                    return WeatherContract.Weather.ContentItemType;

                case WeatherWithLocation:
                    return WeatherContract.Weather.ContentType;

                default:
                    throw new InvalidOperationException("Unknown uri: " + uri);
            }
        }


        public override ICursor Query(Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            // Here's the switch statement that, given a URI, will determine what kind of request it is,
            // and query the database accordingly.
            ICursor retCursor;
            switch (_uriMatcher.Match(uri))
            {
            // "weather/*/*"
                case WeatherWithLocationAndDate:
                    {
                        retCursor = GetWeatherByLocationSettingAndDate(uri, projection, sortOrder);
                        break;
                    }
            // "weather/*"
                case WeatherWithLocation:
                    {
                        retCursor = GetWeatherByLocationSetting(uri, projection, sortOrder);
                        break;
                    }
            // "weather"
                case Weather:
                    {
                        retCursor = _openHelper.ReadableDatabase.Query(WeatherContract.Weather.TableName, 
                            projection, selection, selectionArgs, null, null, sortOrder);
                        break;
                    }
            // "location"
                case Location:
                    {
                        retCursor = _openHelper.ReadableDatabase.Query(WeatherContract.Location.TableName, 
                            projection, selection, selectionArgs, null, null, sortOrder);
                        break;
                    }

                default:
                    throw new InvalidOperationException("Unknown uri: " + uri);
            }
            retCursor.SetNotificationUri(Context.ContentResolver, uri);
            return retCursor;
        }


        /// <summary>
        /// Insert the specified uri and values.
        /// </summary>
        /// <description>
        /// Student: Add the ability to insert Locations to the implementation of this function.
        /// </description>
        public override Android.Net.Uri Insert(Android.Net.Uri uri, ContentValues values)
        {
            var db = _openHelper.WritableDatabase;
            int match = _uriMatcher.Match(uri);
            Android.Net.Uri returnUri;

            switch (match)
            {
                case Weather:
                    {
                        NormalizeDate(values);
                        long _id = db.Insert(WeatherContract.Weather.TableName, null, values);
                        if (_id > 0)
                            returnUri = WeatherContract.Weather.BuildWeatherUri(_id);
                        else
                            throw new Android.Database.SQLException("Failed to insert row into " + uri);
                        break;
                    }
                
                case Location:
                    {
                        long _id = db.Insert(WeatherContract.Location.TableName, null, values);
                        if (_id > 0)
                            returnUri = WeatherContract.Location.BuildLocationUri(_id);
                        else
                            throw new Android.Database.SQLException("Failed to insert row into " + uri);
                        break;
                    }

                default:
                    throw new InvalidOperationException("Unknown uri: " + uri);
            }
            Context.ContentResolver.NotifyChange(uri, null);
            return returnUri;
        }


        public override int Delete(Android.Net.Uri uri, string selection, string[] selectionArgs)
        {

            var db = _openHelper.WritableDatabase;
            int match = _uriMatcher.Match(uri);
            int rowsDeleted;

            // this makes deleted all row return number of rows deleted
            if (selection == null)
                selection = "1";

            switch (match)
            {
                case Weather:
                    {
                        rowsDeleted = db.Delete(WeatherContract.Weather.TableName, selection, selectionArgs);
                        break;
                    }

                case Location:
                    {
                        rowsDeleted = db.Delete(WeatherContract.Location.TableName, selection, selectionArgs);
                        break;
                    }

                default:
                    throw new InvalidOperationException("Unknown uri: " + uri);
            }
            // because a null delete all rows
            if (rowsDeleted != 0)
            {
                Context.ContentResolver.NotifyChange(uri, null);
            }
            return rowsDeleted;

        }

        void NormalizeDate(ContentValues values)
        {
            // normalize the date value
            if (values.ContainsKey(WeatherContract.Weather.ColumnDate))
            {
                long dateValue = values.GetAsLong(WeatherContract.Weather.ColumnDate);
                values.Put(WeatherContract.Weather.ColumnDate, WeatherContract.NormalizeDate(dateValue));
            }
        }


        public override int Update(Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
        {
            var db = _openHelper.WritableDatabase;
            int match = _uriMatcher.Match(uri);
            int rowsUpdated;

            switch (match)
            {
                case Weather:
                    {
                        NormalizeDate(values);
                        rowsUpdated = db.Update(WeatherContract.Weather.TableName, values, selection, selectionArgs);
                        break;
                    }

                case Location:
                    {
                        rowsUpdated = db.Update(WeatherContract.Location.TableName, values, selection, selectionArgs);
                        break;
                    }

                default:
                    throw new InvalidOperationException("Unknown uri: " + uri);
            }
            if (rowsUpdated != 0)
            {
                Context.ContentResolver.NotifyChange(uri, null);
            }
            return rowsUpdated;
        }


        /// <summary>
        /// Bulk insert to by content provider
        /// </summary>
        public override int BulkInsert(Android.Net.Uri uri, ContentValues[] values)
        {
            var db = _openHelper.WritableDatabase;
            int match = _uriMatcher.Match(uri);
            switch (match)
            {
                case Weather:

                    db.BeginTransaction();
                    int returnCount = 0;
                        
                    try
                    {
                        foreach (ContentValues value in values)
                        {
                            NormalizeDate(value);
                            long _id = db.Insert(WeatherContract.Weather.TableName, null, value);
                            if (_id != -1)
                            {
                                returnCount++;
                            }
                        }

                        db.SetTransactionSuccessful();
                    }
                    catch (System.Exception e)
                    {
                        _log.ForContext<WeatherProvider>().Debug("Build insert failed with message {0}", e.Message);    
                    }
                    finally
                    {
                        db.EndTransaction();
                    }
                    Context.ContentResolver.NotifyChange(uri, null);
                    return returnCount;


                default:
                    return base.BulkInsert(uri, values);
            }
        }

           

        // You do not need to call this method. This is a method specifically to assist the testing
        // framework in running smoothly. You can read more at:
        // http://developer.android.com/reference/android/content/ContentProvider.html#shutdown()
        [TargetApi(Value = 11)]
        public override void Shutdown()
        {
            _openHelper.Close();
            base.Shutdown();
        }
    }
}

