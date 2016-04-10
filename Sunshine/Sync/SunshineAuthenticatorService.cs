using System;
using Android.App;

namespace Sunshine.Sync
{

    /// <summary>
    /// The service which allows the sync adapter framework to access the authenticator.
    /// </summary>
    [Service]
    [IntentFilter(new [] { "android.accounts.AccountAuthenticator" })]
    [MetaData("android.accounts.AccountAuthenticator", Resource = "@xml/authenticator")]
    public class SunshineAuthenticatorService : Android.App.Service
    {
        private SunshineAuthenticator _authenticator;


        public override void OnCreate()
        {
            base.OnCreate();
            _authenticator = new SunshineAuthenticator(this);

        }

        /// <summary>
        /// When the system binds to this Service to make the RPC call
        /// return the authenticator's IBinder.
        /// </summary>
        /// <param name="intent">Intent.</param>
        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
            return _authenticator.IBinder;
        }

    }
}

