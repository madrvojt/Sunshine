using System;
using NUnit.Framework;
using Sunshine.Data;
using Android.Content;

namespace Sunshine.Tests.Tests
{


    /// <summary>
    /// Test URI matcher.
    /// </summary>
    /// <description>
    /// Uncomment this class when you are ready to test your UriMatcher.  Note that this class utilizes
    /// constants that are declared with package protection inside of the UriMatcher, which is why
    /// the test must be in the same data package as the Android app code.  Doing the test this way is
    /// a nice compromise between data hiding and testability.
    /// </description>
    [TestFixture]
    public class TestUriMatcher
    {
        const string LocationQuery = "London, UK";
        const long TestDate = 1419033600L;
        // December 20th, 2014
        const long TestLocationId = 10L;

        // content://com.example.android.sunshine.app/weather"
        readonly Android.Net.Uri _testWeatherDir = WeatherContract.Weather.ContentUri;
        readonly Android.Net.Uri _testWeatherWithLocationDir = WeatherContract.Weather.BuildWeatherLocation(LocationQuery);
        readonly Android.Net.Uri _testWeatherWithLocationAndDateDir = WeatherContract.Weather.BuildWeatherLocationWithDate(LocationQuery, TestDate);
        // content://com.example.android.sunshine.app/location"
        readonly Android.Net.Uri _testLocationDir = WeatherContract.Location.ContentUri;


        /// <summary>
        /// URIs the matcher test.
        /// </summary>
        /// <description>
        /// Students: This function tests that your UriMatcher returns the correct integer value
        /// for each of the Uri types that our ContentProvider can handle.  Uncomment this when you are
        /// ready to test your UriMatcher.
        /// </description>
        [Test]
        public void UriMatcherTest()
        {
            var testMatcher = WeatherProvider.BuildUriMatcher();
            Assert.AreEqual(WeatherProvider.Weather, testMatcher.Match(_testWeatherDir), "Error: The WEATHER URI was matched incorrectly.");
            Assert.AreEqual(WeatherProvider.WeatherWithLocation, testMatcher.Match(_testWeatherWithLocationDir), "Error: The WEATHER WITH LOCATION URI was matched incorrectly.");
            Assert.AreEqual(WeatherProvider.WeatherWithLocationAndDate, testMatcher.Match(_testWeatherWithLocationAndDateDir), "Error: The WEATHER WITH LOCATION AND DATE URI was matched incorrectly.");
            Assert.AreEqual(WeatherProvider.Location, testMatcher.Match(_testLocationDir), "Error: The LOCATION URI was matched incorrectly.");
        }
    }
}

