using System;
using Android.Content;
using Sunshine.Data;
using Android.Database.Sqlite;
using NUnit.Framework;
using Android.Database;
using System.Collections.Generic;
using Android.OS;

namespace Sunshine.Tests.Tests
{
    public class TestUtilities
    {
        const String TestLocation = "99705";
        // December 20th, 2014
        const long TestDate = 1419033600L;

        static void ValidateCursor(String error, ICursor valueCursor, ContentValues expectedValues)
        {
            Assert.True(valueCursor.MoveToFirst(), "Empty cursor returned. " + error);
            ValidateCurrentRecord(error, valueCursor, expectedValues);
            valueCursor.Close();
        }

        public static void ValidateCurrentRecord(String error, ICursor valueCursor, ContentValues expectedValues)
        {
            var keySet = expectedValues.KeySet();

            foreach (var key in keySet)
            {
                var columnName = key;
                int idx = valueCursor.GetColumnIndex(columnName);
                Assert.False(idx == -1, "Column '" + columnName + "' not found. " + error);
                var expectedValue = expectedValues.Get(key).ToString();
                Assert.AreEqual(expectedValue, valueCursor.GetString(idx), "Value '" + expectedValues.Get(key) + "' did not match the expected value '" +
                    expectedValue + "'. " + error);

            }
        }

        /// <summary>
        /// Creates the weather values.
        /// </summary>
        /// <description>
        /// Students: Use this to create some default weather values for your database tests.
        /// </description>
        public static ContentValues CreateWeatherValues(long locationRowId)
        {
            var weatherValues = new ContentValues();
            weatherValues.Put(WeatherContract.Weather.ColumnLocationKey, locationRowId);
            weatherValues.Put(WeatherContract.Weather.ColumnDate, TestDate);
            weatherValues.Put(WeatherContract.Weather.ColumnDegrees, 1.1);
            weatherValues.Put(WeatherContract.Weather.ColumnHumidity, 1.2);
            weatherValues.Put(WeatherContract.Weather.ColumnPressure, 1.3);
            weatherValues.Put(WeatherContract.Weather.ColumnMaximumTemp, 75);
            weatherValues.Put(WeatherContract.Weather.ColumnMinimumTemp, 65);
            weatherValues.Put(WeatherContract.Weather.ColumnShortDescription, "Asteroids");
            weatherValues.Put(WeatherContract.Weather.ColumnWindSpeed, 5.5);
            weatherValues.Put(WeatherContract.Weather.ColumnWeatherId, 321);
        
            return weatherValues;
        }


        /// <summary>
        /// Creates the north pole location values.
        /// </summary>
        /// <description> 
        /// Students: You can uncomment this helper function once you have finished creating the
        /// LocationEntry part of the WeatherContract.
        /// </description>
        public static ContentValues CreateNorthPoleLocationValues()
        {
            // Create a new map of values, where column names are the keys
            var testValues = new ContentValues();
            testValues.Put(WeatherContract.Location.ColumnLocationSetting, TestLocation);
            testValues.Put(WeatherContract.Location.ColumnCityName, "North Pole");
            testValues.Put(WeatherContract.Location.ColumnCoordinationLat, 64.7488);
            testValues.Put(WeatherContract.Location.ColumnCoordinationLong, -147.353);

            return testValues;
        }

        /// <summary>
        /// Inserts the north pole location values.
        /// </summary>
        /// <description> 
        /// Students: You can uncomment this function once you have finished creating the
        /// LocationEntry part of the WeatherContract as well as the WeatherDbHelper.
        /// </description>
        static long InsertNorthPoleLocationValues(Context context)
        {
            // insert our test records into the database
            var dbHelper = new WeatherDbHelper(context);
            var db = dbHelper.WritableDatabase;
            ContentValues testValues = TestUtilities.CreateNorthPoleLocationValues();
        
            long locationRowId;
            locationRowId = db.Insert(WeatherContract.Location.TableName, null, testValues);
        
            // Verify we got a row back.
            Assert.True(locationRowId != -1, "Error: Failure to insert North Pole Location Values");
        
            return locationRowId;
        }
    }
}
