using System;
using NUnit.Framework;
using Android.Content;
using Sunshine.Data;
using Android.Database.Sqlite;
using Android.Database;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Sunshine.Tests.Tests
{
    [TestFixture]
    public class TestDb
    {
        Context _context;

        /// <summary>
        /// Setup this instance.
        /// </summary>
        /// <description>
        ///  This function gets called before each test is executed to delete the database.  This makes
        //   sure that we always have a clean test.
        /// </description>
        [SetUp]
        public void Setup()
        {
            _context = Android.App.Application.Context;
            DeleteTheDatabase();
        
        }

       
        // Since we want each test to start with a clean slate
        void DeleteTheDatabase()
        {
            _context.DeleteDatabase(WeatherDbHelper.Database);
        }

        /// <summary>
        /// Tests the create db.
        /// </summary>
        /// <description>
        /// Build a HashSet of all of the table names we wish to look for
        /// Note that there will be another table in the DB that stores the
        /// Android metadata (db version information)
        /// Students: Uncomment this test once you've written the code to create the Location
        /// table.  Note that you will have to have chosen the same column names that I did in
        /// my solution for this test to compile, so if you haven't yet done that, this is
        /// a good time to change your column names to match mine.
        /// Note that this only tests that the Location table has the correct columns, since we
        /// give you the code for the weather table.  This test does not look at the
        /// </description>
        [Test]
        public void TestCreateDb()
        {
            var tableNameHashSet = new HashSet<String>();
            tableNameHashSet.Add(WeatherContract.Location.TableName);
            tableNameHashSet.Add(WeatherContract.Weather.TableName);
        
            _context.DeleteDatabase(WeatherDbHelper.Database);
            SQLiteDatabase db = new WeatherDbHelper(_context).ReadableDatabase;
            Assert.AreEqual(true, db.IsOpen);
        
            // have we created the tables we want?
            ICursor c = db.RawQuery("SELECT name FROM sqlite_master WHERE type='table'", null);
            Assert.True(c.MoveToFirst(), "Error: This means that the database has not been created correctly");
        
            do
            {
                Debug.WriteLine(c.GetString(0));
            } while(c.MoveToNext());

            c.MoveToFirst();

            // verify that the tables have been created
            do
            {
                tableNameHashSet.Remove(c.GetString(0));
            } while(c.MoveToNext());
        
            // if this fails, it means that your database doesn't contain both the location entry
            // and weather entry tables
            Assert.True(!tableNameHashSet.Any(), "Error: Your database was created without both the location entry and weather entry tables");
        
            // now, do our tables contain the correct columns
            c = db.RawQuery("PRAGMA table_info(" + WeatherContract.Location.TableName + ")",
                null);
        
            Assert.True(c.MoveToFirst(), "Error: This means that we were unable to query the database for table information.");
        
            // Build a HashSet of all of the column names we want to look for
            var locationColumnHashSet = new HashSet<String>();
            locationColumnHashSet.Add(WeatherContract.Location.Id);
            locationColumnHashSet.Add(WeatherContract.Location.ColumnCityName);
            locationColumnHashSet.Add(WeatherContract.Location.ColumnCoordinationLat);
            locationColumnHashSet.Add(WeatherContract.Location.ColumnCoordinationLong);
            locationColumnHashSet.Add(WeatherContract.Location.ColumnLocationSetting);
        
            // Identificator of name of column in database schema
            const string Name = "name";
            // Name of table
            int columnNameIndex = c.GetColumnIndex(Name);
            do
            {
                var columnName = c.GetString(columnNameIndex);
                locationColumnHashSet.Remove(columnName);
            } while(c.MoveToNext());
        
            // if this fails, it means that your database doesn't contain all of the required location
            // entry columns
            Assert.True(!locationColumnHashSet.Any(), "Error: The database doesn't contain all of the required location entry columns");
            db.Close();
        }

        [Test]
        /// <summary>
        /// Tests the location table.
        /// </summary>
        /// <description>
        /// Students:  Here is where you will build code to test that we can insert and query the
        /// location database.  We've done a lot of work for you.  You'll want to look in TestUtilities
        /// where you can uncomment out the "createNorthPoleLocationValues" function.  You can
        /// also make use of the ValidateCurrentRecord function from within TestUtilities.
        /// </description>
        public void TestLocationTable()
        {
            InsertLocation();
        }

        [Test]
        /// <summary>
        /// Tests the weather table.
        /// </summary>
        /// <description>
        /// Students:  Here is where you will build code to test that we can insert and query the
        /// database.  We've done a lot of work for you.  You'll want to look in TestUtilities
        /// where you can use the "createWeatherValues" function.  You can
        /// also make use of the validateCurrentRecord function from within TestUtilities.
        /// </description>
        public void TestWeatherTable()
        {
            // Insert location weather row ID for foregion key
            var locationRowId = InsertLocation();

            // First step: Get reference to writable database
            var db = new WeatherDbHelper(_context).WritableDatabase;
            var weatherContentValues = TestUtilities.CreateWeatherValues(locationRowId);
            // Return new location ID
            var weatherRowId = db.Insert(WeatherContract.Weather.TableName, null, weatherContentValues);
            Assert.True(weatherRowId != -1);
            // Select cursor
            var cursor = db.Query(WeatherContract.Weather.TableName, null, null, null, null, null, null); 
            Assert.True(cursor.MoveToFirst(), "Error: No record returns from location query");
            // Validation cursor data with contentValues
            TestUtilities.ValidateCurrentRecord("Error: Location query validation failed", cursor, weatherContentValues);
            Assert.False(cursor.MoveToNext(), "Error: More then one record returned from location query");
            // Close cursor and database
            cursor.Close();
            db.Close();
        }

        /// <summary>
        /// Inserts the location.
        /// </summary>
        /// <description>
        /// Students: This is a helper method for the testWeatherTable quiz. You can move your
        //  code from testLocationTable to here so that you can call this code from both
        /// </description>
        public long InsertLocation()
        {
            // First step: Get reference to writable database
            SQLiteDatabase db = new WeatherDbHelper(_context).WritableDatabase;
            var contentValues = TestUtilities.CreateNorthPoleLocationValues();
            // Return new location ID
            var locationRowId = db.Insert(WeatherContract.Location.TableName, null, contentValues);
            Assert.True(locationRowId != -1);
            // Select cursor
            var cursor = db.Query(WeatherContract.Location.TableName, null, null, null, null, null, null); 
            Assert.True(cursor.MoveToFirst(), "Error: No record returns from location query");
            // Validation cursor data with contentValues
            TestUtilities.ValidateCurrentRecord("Error: Location query validation failed", cursor, contentValues);
            Assert.False(cursor.MoveToNext(), "Error: More then one record returned from location query");
            // Close cursor and database
            cursor.Close();
            db.Close();

            return locationRowId;
        }
    }
}

