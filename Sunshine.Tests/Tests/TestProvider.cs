using NUnit.Framework;
using Android.Content;
using Sunshine.Data;
using Android.Database.Sqlite;
using Android.Database;
using Android.Content.PM;
using Java.Lang;
using Android.OS;

namespace Sunshine.Tests.Tests
{

    /// <summary>
    /// Test provider.
    /// </summary>
    /// <description>
    ///  Note: This is not a complete set of tests of the Sunshine ContentProvider, but it does test
    /// that at least the basic functionality has been implemented correctly.
    /// Students: Uncomment the tests in this class as you implement the functionality in your
    /// ContentProvider to make sure that you've implemented things reasonably correctly.
    /// </description>
    [TestFixture]
    public class TestProvider
    {
        Context _context;
        const int BulkInsertRecordsToInsert = 2;


        // This helper function deletes all records from both database tables using the ContentProvider.
        // It also queries the ContentProvider to make sure that the database has been successfully
        // deleted, so it cannot be used until the Query and Delete functions have been written
        // in the ContentProvider.
        [SetUp]
        public void Setup()
        {
            _context = Android.App.Application.Context;
            DeleteAllRecords();
        }

        /// <summary>
        /// Deletes all records from provider.
        /// </summary>
        /// <description>
        ///  Students: Replace the calls to deleteAllRecordsFromDB with this one after you have written
        /// the delete functionality in the ContentProvider.
        /// </description>
        [Test]
        public void DeleteAllRecordsFromProvider()
        {
            _context.ContentResolver.Delete(WeatherContract.Weather.ContentUri,
                null,
                null
            );
            _context.ContentResolver.Delete(
                WeatherContract.Location.ContentUri,
                null,
                null
            );

            ICursor cursor = _context.ContentResolver.Query(
                                 WeatherContract.Weather.ContentUri,
                                 null,
                                 null,
                                 null,
                                 null
                             );
            Assert.AreEqual(0, cursor.Count, "Error: Records not deleted from Weather table during delete");
            cursor.Close();

            cursor = _context.ContentResolver.Query(
                WeatherContract.Location.ContentUri,
                null,
                null,
                null,
                null
            );
            Assert.AreEqual(0, cursor.Count, "Error: Records not deleted from Location table during delete");
            cursor.Close();
        }

        /// <summary>
        /// Deletes all records from D.
        /// </summary>
        /// <description>
        /// This helper function deletes all records from both database tables using the database
        /// functions only.  This is designed to be used to reset the state of the database until the
        /// delete functionality is available in the ContentProvider.
        /// </description>
        [Test]    
        public void DeleteAllRecordsFromDB()
        {
            var dbHelper = new WeatherDbHelper(_context);
            var db = dbHelper.WritableDatabase;

            db.Delete(WeatherContract.Weather.TableName, null, null);
            db.Delete(WeatherContract.Location.TableName, null, null);
            db.Close();
        }

        /// <summary>
        /// Deletes all records.
        /// </summary>
        /// <description>
        /// Student: Refactor this function to use the deleteAllRecordsFromProvider functionality once
        /// you have implemented delete functionality there.
        /// </description>
        ///         
        [Test]
        public void DeleteAllRecords()
        {
            DeleteAllRecordsFromProvider();
        }

        /// <summary>
        /// Tests the provider registry.
        /// </summary>
        /// <description>
        /// This test checks to make sure that the content provider is registered correctly.
        /// Students: Uncomment this test to make sure you've correctly registered the WeatherProvider.
        /// </description>
        [Test]
        public void TestProviderRegistry()
        {
            var pm = _context.PackageManager;
            
            // We define the component name based on the package name from the context and the
            // WeatherProvider class.
            var componentName = new ComponentName(_context, Class.FromType(typeof(WeatherProvider)));
            try
            {
                // Fetch the provider info using the component name from the PackageManager
                // This throws an exception if the provider isn't registered.
                ProviderInfo providerInfo = pm.GetProviderInfo(componentName, 0);
            
                // Make sure that the registered authority matches the authority from the Contract.
                Assert.AreEqual(providerInfo.Authority, WeatherContract.ContentAuthority, string.Format("Error: WeatherProvider registered with authority: {0} instead of authority: {1}", providerInfo.Authority, WeatherContract.ContentAuthority));
            }
            catch (PackageManager.NameNotFoundException)
            {
                // I guess the provider isn't registered correctly.
                Assert.True(false, "Error: WeatherProvider not registered at " + _context.PackageName);
            }
        }

        /// <summary>
        /// Tests the type of the get.
        /// </summary>
        /// <description>
        /// This test doesn't touch the database.  It verifies that the ContentProvider returns
        /// the correct type for each type of URI that it can handle.
        /// Students: Uncomment this test to verify that your implementation of GetType is
        /// functioning correctly.
        /// </description>
        [Test]
        public void TestGetType()
        {
            // content://com.example.android.sunshine.app/weather/
            string type = _context.ContentResolver.GetType(WeatherContract.Weather.ContentUri);
            // vnd.android.cursor.dir/com.example.android.sunshine.app/weather
            Assert.AreEqual(WeatherContract.Weather.ContentType, type, "Error: the WeatherEntry CONTENT_URI should return WeatherEntry.CONTENT_TYPE");
        
            const string testLocation = "94074";
            // content://com.example.android.sunshine.app/weather/94074
            type = _context.ContentResolver.GetType(WeatherContract.Weather.BuildWeatherLocation(testLocation));

            // vnd.android.cursor.dir/com.example.android.sunshine.app/weather
            Assert.AreEqual(WeatherContract.Weather.ContentType, type, "Error: the WeatherEntry CONTENT_URI with location should return WeatherEntry.CONTENT_TYPE");
        
            const long testDate = 1419120000L; // December 21st, 2014
            // content://com.example.android.sunshine.app/weather/94074/20140612
            type = _context.ContentResolver.GetType(
                WeatherContract.Weather.BuildWeatherLocationWithDate(testLocation, testDate));
            // vnd.android.cursor.item/com.example.android.sunshine.app/weather/1419120000
            Assert.AreEqual(WeatherContract.Weather.ContentItemType, type, "Error: the WeatherEntry CONTENT_URI with location and date should return WeatherEntry.CONTENT_ITEM_TYPE");
        
            // content://com.example.android.sunshine.app/location/
            type = _context.ContentResolver.GetType(WeatherContract.Location.ContentUri);
            // vnd.android.cursor.dir/com.example.android.sunshine.app/location
            Assert.AreEqual(WeatherContract.Location.ContentType, type, "Error: the LocationEntry CONTENT_URI should return LocationEntry.CONTENT_TYPE");
        }

        /// <summary>
        /// Tests the basic weather query.
        /// </summary>
        /// <description>
        /// This test uses the database directly to insert and then uses the ContentProvider to
        /// read out the data.  Uncomment this test to see if the basic weather query functionality
        /// given in the ContentProvider is working correctly.
        /// </description>
        [Test]    
        public void TestBasicWeatherQuery()
        {
            // insert our test records into the database
            var dbHelper = new WeatherDbHelper(_context);
            SQLiteDatabase db = dbHelper.WritableDatabase;
        
            long locationRowId = TestUtilities.InsertNorthPoleLocationValues(_context);
        
            // Fantastic.  Now that we have a location, add some weather!
            var weatherValues = TestUtilities.CreateWeatherValues(locationRowId);
        
            long weatherRowId = db.Insert(WeatherContract.Weather.TableName, null, weatherValues);
            Assert.True(weatherRowId != -1, "Unable to Insert WeatherEntry into the Database");
        
            db.Close();
        
            // Test the basic content provider query
            ICursor weatherCursor = _context.ContentResolver.Query(
                                        WeatherContract.Weather.ContentUri,
                                        null,
                                        null,
                                        null,
                                        null
                                    );
        
            // Make sure we get the correct cursor out of the database
            TestUtilities.ValidateCursor("testBasicWeatherQuery", weatherCursor, weatherValues);
        }

        /// <summary>
        /// Tests the basic location queries.
        /// </summary>
        /// <description>
        /// This test uses the database directly to insert and then uses the ContentProvider to
        /// read out the data.  Uncomment this test to see if your location queries are
        /// performing correctly.
        /// </description>
        [Test]
        public void TestBasicLocationQueries()
        {
            // insert our test records into the database
            var dbHelper = new WeatherDbHelper(_context);
            var db = dbHelper.WritableDatabase;
        
            var testValues = TestUtilities.CreateNorthPoleLocationValues();
            TestUtilities.InsertNorthPoleLocationValues(_context);
        
            // Test the basic content provider query
            var locationCursor = _context.ContentResolver.Query(
                                     WeatherContract.Location.ContentUri,
                                     null,
                                     null,
                                     null,
                                     null
                                 );
        
            // Make sure we get the correct cursor out of the database
            TestUtilities.ValidateCursor("testBasicLocationQueries, location query", locationCursor, testValues);
        
            // Has the NotificationUri been set correctly? --- we can only test this easily against API
            // level 19 or greater because getNotificationUri was added in API level 19.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Assert.AreEqual(locationCursor.NotificationUri.ToString(), WeatherContract.Location.ContentUri.ToString(), "Error: Location Query did not properly set NotificationUri");
            }
        }

        /// <summary>
        /// Tests the update location.
        /// </summary>
        /// <description>
        /// This test uses the provider to insert and then update the data. Uncomment this test to
        /// see if your update location is functioning correctly.
        /// </description>
        [Test]
        public void TestUpdateLocation()
        {
            // Create a new map of values, where column names are the keys
            var values = TestUtilities.CreateNorthPoleLocationValues();
        
            Android.Net.Uri locationUri = _context.ContentResolver.Insert(WeatherContract.Location.ContentUri, values);
            long locationRowId = ContentUris.ParseId(locationUri);
        
            // Verify we got a row back.
            Assert.True(locationRowId != -1);
            //TODO: Logger
            // DeLOG_TAG, "New row id: " + locationRowId);
        
            var updatedValues = new ContentValues(values);
            updatedValues.Put(WeatherContract.Location.Id, locationRowId);
            updatedValues.Put(WeatherContract.Location.ColumnCityName, "Santa's Village");
        
            // Create a cursor with observer to make sure that the content provider is notifying
            // the observers as expected
            ICursor locationCursor = _context.ContentResolver.Query(WeatherContract.Location.ContentUri, null, null, null, null);
        
            TestContentObserver tco = TestContentObserver.GetTestContentObserver();
            locationCursor.RegisterContentObserver(tco);
        
            int count = _context.ContentResolver.Update(
                            WeatherContract.Location.ContentUri, updatedValues, WeatherContract.Location.Id + "= ?",
                            new string[] { locationRowId.ToString() });
            Assert.AreEqual(count, 1);
        
            // Test to make sure our observer is called.  If not, we throw an assertion.
            //
            // Students: If your code is failing here, it means that your content provider
            // isn't calling getContext().getContentResolver().notifyChange(uri, null);
            tco.WaitForNotificationOrFail();
        
            locationCursor.UnregisterContentObserver(tco);
            locationCursor.Close();
        
            // A cursor is your primary interface to the query results.
            ICursor cursor = _context.ContentResolver.Query(
                                 WeatherContract.Location.ContentUri,
                                 null,   // projection
                                 WeatherContract.Location.Id + " = " + locationRowId,
                                 null,   // Values for the "where" clause
                                 null    // sort order
                             );
        
            TestUtilities.ValidateCursor("testUpdateLocation.  Error validating location entry update.",
                cursor, updatedValues);
        
            cursor.Close();
        }


        // Make sure we can still delete after adding/updating stuff
        //
        // Student: Uncomment this test after you have completed writing the insert functionality
        // in your provider.  It relies on insertions with testInsertReadProvider, so insert and
        // query functionality must also be complete before this test can be used.
        [Test]    
        public void TestInsertReadProvider()
        {
            var testValues = TestUtilities.CreateNorthPoleLocationValues();
        
            // Register a content observer for our insert.  This time, directly with the content resolver
            var testContentObserver = TestContentObserver.GetTestContentObserver();
            _context.ContentResolver.RegisterContentObserver(WeatherContract.Location.ContentUri, true, testContentObserver);
            Android.Net.Uri locationUri = _context.ContentResolver.Insert(WeatherContract.Location.ContentUri, testValues);
        
            // Did our content observer get called?  Students:  If this fails, your insert location
            // isn't calling getContext().getContentResolver().notifyChange(uri, null);
            testContentObserver.WaitForNotificationOrFail();
            _context.ContentResolver.UnregisterContentObserver(testContentObserver);
        
            long locationRowId = ContentUris.ParseId(locationUri);
        
            // Verify we got a row back.
            Assert.True(locationRowId != -1);
        
            // Data's inserted.  IN THEORY.  Now pull some out to stare at it and verify it made
            // the round trip.
        
            // A cursor is your primary interface to the query results.
            var cursor = _context.ContentResolver.Query(
                             WeatherContract.Location.ContentUri,
                             null, // leaving "columns" null just returns all the columns.
                             null, // cols for "where" clause
                             null, // values for "where" clause
                             null  // sort order
                         );
        
            TestUtilities.ValidateCursor("testInsertReadProvider. Error validating LocationEntry.",
                cursor, testValues);
        
            // Fantastic.  Now that we have a location, add some weather!
            var weatherValues = TestUtilities.CreateWeatherValues(locationRowId);
            // The TestContentObserver is a one-shot class
            testContentObserver = TestContentObserver.GetTestContentObserver();
        
            _context.ContentResolver.RegisterContentObserver(WeatherContract.Weather.ContentUri, true, testContentObserver);
        
            Android.Net.Uri weatherInsertUri = _context.ContentResolver.Insert(WeatherContract.Weather.ContentUri, weatherValues);
            Assert.True(weatherInsertUri != null);
        
            // Did our content observer get called?  Students:  If this fails, your insert weather
            // in your ContentProvider isn't calling
            // getContext().getContentResolver().notifyChange(uri, null);
            testContentObserver.WaitForNotificationOrFail();
            _context.ContentResolver.UnregisterContentObserver(testContentObserver);
        
            // A cursor is your primary interface to the query results.
            var weatherCursor = _context.ContentResolver.Query(
                                    WeatherContract.Weather.ContentUri,  // Table to Query
                                    null, // leaving "columns" null just returns all the columns.
                                    null, // cols for "where" clause
                                    null, // values for "where" clause
                                    null // columns to group by
                                );
        
            TestUtilities.ValidateCursor("testInsertReadProvider. Error validating WeatherEntry insert.",
                weatherCursor, weatherValues);
        
            // Add the location values in with the weather data so that we can make
            // sure that the join worked and we actually get all the values back
            weatherValues.PutAll(testValues);
        
            // Get the joined Weather and Location data
            weatherCursor = _context.ContentResolver.Query(
                WeatherContract.Weather.BuildWeatherLocation(TestUtilities.TestLocation),
                null, // leaving "columns" null just returns all the columns.
                null, // cols for "where" clause
                null, // values for "where" clause
                null  // sort order
            );
            TestUtilities.ValidateCursor("testInsertReadProvider.  Error validating joined Weather and Location Data.",
                weatherCursor, weatherValues);
        
            // Get the joined Weather and Location data with a start date
            weatherCursor = _context.ContentResolver.Query(
                WeatherContract.Weather.BuildWeatherLocationWithStartDate(
                    TestUtilities.TestLocation, TestUtilities.TestDate),
                null, // leaving "columns" null just returns all the columns.
                null, // cols for "where" clause
                null, // values for "where" clause
                null  // sort order
            );
            TestUtilities.ValidateCursor("testInsertReadProvider.  Error validating joined Weather and Location Data with start date.",
                weatherCursor, weatherValues);
        
            // Get the joined Weather data for a specific date
            weatherCursor = _context.ContentResolver.Query(
                WeatherContract.Weather.BuildWeatherLocationWithDate(TestUtilities.TestLocation, TestUtilities.TestDate),
                null,
                null,
                null,
                null
            );
            TestUtilities.ValidateCursor("testInsertReadProvider.  Error validating joined Weather and Location data for a specific date.",
                weatherCursor, weatherValues);
        }

        /// <summary>
        /// Make sure we can still delete after adding/updating stuff
        /// </summary>
        /// <description>
        /// Student: Uncomment this test after you have completed writing the delete functionality
        /// in your provider.  It relies on insertions with testInsertReadProvider, so insert and
        /// query functionality must also be complete before this test can be used.
        /// </description>
        [Test]
        public void TestDeleteRecords()
        {
            TestInsertReadProvider();
        
            // Register a content observer for our location delete.
            var locationObserver = TestContentObserver.GetTestContentObserver();
            _context.ContentResolver.RegisterContentObserver(WeatherContract.Location.ContentUri, true, locationObserver);
        
            // Register a content observer for our weather delete.
            var weatherObserver = TestContentObserver.GetTestContentObserver();
            _context.ContentResolver.RegisterContentObserver(WeatherContract.Weather.ContentUri, true, weatherObserver);
        
            DeleteAllRecordsFromProvider();
        
            // Students: If either of these fail, you most-likely are not calling the
            // getContext().getContentResolver().notifyChange(uri, null); in the ContentProvider
            // delete.  (only if the insertReadProvider is succeeding)
            locationObserver.WaitForNotificationOrFail();
            weatherObserver.WaitForNotificationOrFail();
        
            _context.ContentResolver.UnregisterContentObserver(locationObserver);
            _context.ContentResolver.UnregisterContentObserver(weatherObserver);
        }


            
        static ContentValues[] CreateBulkInsertWeatherValues(long locationRowId)
        {
            long currentTestDate = TestUtilities.TestDate;
            const long millisecondsInADay = 1000 * 60 * 60 * 24;
            var returnContentValues = new ContentValues[BulkInsertRecordsToInsert];
        
            for (int i = 0; i < BulkInsertRecordsToInsert; i++)
            {
                var weatherValues = new ContentValues();
                weatherValues.Put(WeatherContract.Weather.ColumnLocationKey, locationRowId);
                currentTestDate += millisecondsInADay;
                weatherValues.Put(WeatherContract.Weather.ColumnDate, currentTestDate);
                weatherValues.Put(WeatherContract.Weather.ColumnDegrees, 1.1);
                weatherValues.Put(WeatherContract.Weather.ColumnHumidity, 1.2 + 0.01 * (float)i);
                weatherValues.Put(WeatherContract.Weather.ColumnPressure, 1.3 - 0.01 * (float)i);
                weatherValues.Put(WeatherContract.Weather.ColumnMaximumTemp, 75 + i);
                weatherValues.Put(WeatherContract.Weather.ColumnMinimumTemp, 65 - i);
                weatherValues.Put(WeatherContract.Weather.ColumnShortDescription, "Asteroids");
                weatherValues.Put(WeatherContract.Weather.ColumnWindSpeed, 5.5 + 0.2 * (float)i);
                weatherValues.Put(WeatherContract.Weather.ColumnWeatherId, 321);
                returnContentValues[i] = weatherValues;
            }
            return returnContentValues;
        }

 
        /// <summary>
        /// Tests the bulk insert.
        /// </summary>
        /// <description>
        /// Student: Uncomment this test after you have completed writing the BulkInsert functionality
        /// in your provider.  Note that this test will work with the built-in (default) provider
        /// implementation, which just inserts records one-at-a-time, so really do implement the
        /// BulkInsert ContentProvider function.
        /// </description>
        [Test]
        public void TestBulkInsert()
        {
            // first, let's create a location value
            ContentValues testValues = TestUtilities.CreateNorthPoleLocationValues();
            var locationUri = _context.ContentResolver.Insert(WeatherContract.Location.ContentUri, testValues);
            long locationRowId = ContentUris.ParseId(locationUri);
            
            // Verify we got a row back.
            Assert.True(locationRowId != -1);
            
            // Data's inserted.  IN THEORY.  Now pull some out to stare at it and verify it made
            // the round trip.
            
            // A cursor is your primary interface to the query results.
            var cursorLocation = _context.ContentResolver.Query(
                                     WeatherContract.Location.ContentUri,
                                     null, // leaving "columns" null just returns all the columns.
                                     null, // cols for "where" clause
                                     null, // values for "where" clause
                                     null  // sort order
                                 );
            
            TestUtilities.ValidateCursor("testBulkInsert. Error validating LocationEntry.",
                cursorLocation, testValues);


            cursorLocation.Close();

            // Now we can bulkInsert some weather.  In fact, we only implement BulkInsert for weather
            // entries.  With ContentProviders, you really only have to implement the features you
            // use, after all.
            ContentValues[] bulkInsertContentValues = CreateBulkInsertWeatherValues(locationRowId);
            
            // Register a content observer for our bulk insert.
            TestContentObserver weatherObserver = TestContentObserver.GetTestContentObserver();
            _context.ContentResolver.RegisterContentObserver(WeatherContract.Weather.ContentUri, true, weatherObserver);
            
            int insertCount = _context.ContentResolver.BulkInsert(WeatherContract.Weather.ContentUri, bulkInsertContentValues);
            
            // Students:  If this fails, it means that you most-likely are not calling the
            // getContext().getContentResolver().notifyChange(uri, null); in your BulkInsert
            // ContentProvider method.
            weatherObserver.WaitForNotificationOrFail();
            _context.ContentResolver.UnregisterContentObserver(weatherObserver);
            
            Assert.AreEqual(insertCount, BulkInsertRecordsToInsert);
            
            // A cursor is your primary interface to the query results.
            var cursorWeather = _context.ContentResolver.Query(
                                    WeatherContract.Weather.ContentUri,
                                    null, // leaving "columns" null just returns all the columns.
                                    null, // cols for "where" clause
                                    null, // values for "where" clause
                                    WeatherContract.Weather.ColumnDate + " ASC"  // sort order == by DATE ASCENDING
                                );


            // we should have as many records in the database as we've inserted
            Assert.AreEqual(cursorWeather.Count, BulkInsertRecordsToInsert);
            
            // and let's make sure they match the ones we created
            cursorWeather.MoveToFirst();
            for (int i = 0; i < BulkInsertRecordsToInsert; i++, cursorWeather.MoveToNext())
            {
                TestUtilities.ValidateCurrentRecord("testBulkInsert.  Error validating WeatherEntry " + i,
                    cursorWeather, bulkInsertContentValues[i]);
            }
            cursorWeather.Close();
        }
    }
}

