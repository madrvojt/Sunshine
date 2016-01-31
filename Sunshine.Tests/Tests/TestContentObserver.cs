using System;
using Android.Database;
using Android.OS;

namespace Sunshine.Tests.Tests
{
    class TestContentObserver : ContentObserver
    {
        readonly HandlerThread _hT;
        bool _contentChanged;

        static TestContentObserver GetTestContentObserver()
        {
            var ht = new HandlerThread("ContentObserverThread");
            ht.Start();
            return new TestContentObserver(ht);
        }

        private TestContentObserver(HandlerThread ht)
            : base(new Handler(ht.Looper))
        {
            _hT = ht;
        }

        /// <summary>
        /// Raises the change event.
        /// </summary>
        /// <description>
        /// Students: The functions we provide inside of TestProvider use this utility class to test
        /// the ContentObserver callbacks using the PollingCheck class that we grabbed from the Android
        /// CTS tests.
        /// Note that this only tests that the onChange function is called; it does not test that the
        /// correct Uri is returned.
        /// </description>
        public override void OnChange(bool selfChange)
        {
            base.OnChange(selfChange, null);
        }

        public override void OnChange(bool selfChange, Android.Net.Uri uri)
        {
            _contentChanged = true;
        }

        public void WaitForNotificationOrFail()
        {
            // Note: The PollingCheck class is taken from the Android CTS (Compatibility Test Suite).
            // It's useful to look at the Android CTS source for ideas on how to test your Android
            // applications.  The reason that PollingCheck works is that, by default, the JUnit
            // testing framework is not running on the main Android application thread.
            new PollingCheck(5000)
            {
                Check = _contentChanged

            }.Run();

            _hT.Quit();
        }

    }

}

