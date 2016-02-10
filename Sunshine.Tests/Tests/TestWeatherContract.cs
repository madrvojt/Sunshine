using System;
using NUnit.Framework;
using Sunshine.Data;

namespace Sunshine.Tests.Tests
{

    /// <summary>
    /// Test weather contract.
    /// </summary>
    /// <description>
    /// Students: This is NOT a complete test for the WeatherContract --- just for the functions
    /// that we expect you to write.
    /// </description>
    [TestFixture]
    public class TestWeatherContract
    {

        // intentionally includes a slash to make sure Uri is getting quoted correctly
        const string TestWeatherLocation = "/North Pole";
        const long TestWeatherDate = 1419033600L;
        // December 20th, 2014

        /// <summary>
        /// Tests the build weather location.
        /// </summary>
        /// <description>
        /// Students: Uncomment this out to test your weather location function.
        /// </description>
        [Test]
        public void TestBuildWeatherLocation()
        {
            Android.Net.Uri locationUri = WeatherContract.Weather.BuildWeatherLocation(TestWeatherLocation);
            Assert.NotNull(locationUri, "Error: Null Uri returned.  You must fill-in buildWeatherLocation in " +
                "WeatherContract."
            );
            Assert.AreEqual(TestWeatherLocation, locationUri.LastPathSegment, "Error: Weather location not properly appended to the end of the Uri");
            Assert.AreEqual(locationUri.ToString(),
                "content://com.example.android.sunshine.app/weather/%2FNorth%20Pole", "Error: Weather location Uri doesn't match our expected result");
        }
    }
}

