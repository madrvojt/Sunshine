using System;
using Android.Database;
using Android.OS;
using System.Threading.Tasks;

namespace Sunshine.Tests.Tests
{
    public class TestContentObserver : ContentObserver
    {
        readonly HandlerThread _hT;
        bool _contentChanged;

        public static TestContentObserver GetTestContentObserver()
        {
            var ht = new HandlerThread("ContentObserverThread");
            ht.Start();
            return new TestContentObserver(ht);
        }

        TestContentObserver(HandlerThread ht)
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

        public async void WaitForNotificationOrFail()
        {
            // Note: The PollingCheck class is change to classic delay
            await Task.Delay(5000);
            if (!_contentChanged)
                throw new InvalidOperationException();

            _hT.Quit();
        }

    }

}

