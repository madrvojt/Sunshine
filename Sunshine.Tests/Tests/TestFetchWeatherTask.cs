using System;
using NUnit.Framework;
using Android.Content;
using Sunshine.Data;
using Android.Database;

namespace Sunshine.Tests.Tests
{
    [TestFixture]
    public class TestFetchWeatherTask
    {
        const string AddLocationSetting = "Sunnydale, CA";
        const string AddLocationCity = "Sunnydale";
        const double AddLocationLatitude = 34.425833;
        const double AddLocationLongtitude = -119.714167;

        Context _context;

        [SetUp]
        public void Setup()
        {
            _context = Android.App.Application.Context;
        }

        /// <summary>
        /// Tests the add location.
        /// </summary>
        [Test]
        public void TestAddLocation()
        {
            // start from a clean state
            _context.ContentResolver.Delete(WeatherContract.Location.ContentUri,
                WeatherContract.Location.ColumnLocationSetting + " = ?",
                new String[]{ AddLocationSetting });
        
            var fetchWeatherTask = new FetchWeatherTask(_context);
            long locationId = fetchWeatherTask.AddLocation(AddLocationSetting, AddLocationCity,
                                  AddLocationLatitude, AddLocationLongtitude);
        
            // does addLocation return a valid record ID?
            Assert.False(locationId == -1, "Error: addLocation returned an invalid ID on insert");
        
            // test all this twice
            for (int i = 0; i < 2; i++)
            {
        
                // does the ID point to our location?
                ICursor locationCursor = _context.ContentResolver.Query(
                                             WeatherContract.Location.ContentUri,
                                             new String[]
                    {
                        WeatherContract.Location.Id,
                        WeatherContract.Location.ColumnLocationSetting,
                        WeatherContract.Location.ColumnCityName,
                        WeatherContract.Location.ColumnCoordinationLat,
                        WeatherContract.Location.ColumnCoordinationLong
                    },
                                             WeatherContract.Location.ColumnLocationSetting + " = ?",
                                             new string[]{ AddLocationSetting },
                                             null);
        
                // these match the indices of the projection
                if (locationCursor.MoveToFirst())
                {
                    Assert.AreEqual(locationCursor.GetLong(0), locationId, "Error: the queried value of locationId does not match the returned value" + "from addLocation");
                    Assert.AreEqual(locationCursor.GetString(1), AddLocationSetting, "Error: the queried value of location setting is incorrect");
                    Assert.AreEqual(locationCursor.GetString(2), AddLocationCity, "Error: the queried value of location city is incorrect");
                    Assert.AreEqual(locationCursor.GetDouble(3), AddLocationLatitude, "Error: the queried value of latitude is incorrect");
                    Assert.AreEqual(locationCursor.GetDouble(4), AddLocationLongtitude, "Error: the queried value of longitude is incorrect");
                }
                else
                {
                    Assert.Fail("Error: the id you used to query returned an empty cursor");
                }
        
                // there should be no more records
                Assert.False(locationCursor.MoveToNext(), "Error: there should be only one record returned from a location query");
        
                // add the location again
                long newLocationId = fetchWeatherTask.AddLocation(AddLocationSetting, AddLocationCity,
                                         AddLocationLatitude, AddLocationLongtitude);
        
                Assert.AreEqual(locationId, newLocationId, "Error: inserting a location again should return the same ID");
            }
            // reset our state back to normal
            _context.ContentResolver.Delete(WeatherContract.Location.ContentUri,
                WeatherContract.Location.ColumnLocationSetting + " = ?",
                new []{ AddLocationSetting });
        
            // clean up the test so that other tests can use the content provider
            _context.ContentResolver.AcquireContentProviderClient(WeatherContract.Location.ContentUri).LocalContentProvider.Shutdown();
        }
    }
}

