using System;
using Android.Accounts;
using Android.Content;

namespace Sunshine.Sync
{
    /// <summary>
    /// Manages "Authentication" to Sunshine's backend service.  The SyncAdapter framework
    /// requires an authenticator object, so syncing to a service that doesn't need authentication
    /// typically means creating a stub authenticator like this one.
    /// This code is copied directly, in its entirety, from
    /// http://developer.android.com/training/sync-adapters/creating-authenticator.html
    /// Which is a pretty handy reference when creating your own syncadapters.  Just sayin'.
    /// </summary>
    public class SunshineAuthenticator : AbstractAccountAuthenticator
    {

            
        public SunshineAuthenticator(Context context)
            : base(context)
        {
        }


        public override Android.OS.Bundle EditProperties(AccountAuthenticatorResponse response, string accountType)
        {
            throw new NotImplementedException();
        }

        // Because we're not actually adding an account to the device, just return null.
        public override Android.OS.Bundle AddAccount(AccountAuthenticatorResponse response, string accountType, string authTokenType, string[] requiredFeatures, Android.OS.Bundle options)
        {
            return null;
        }
            
        // Ignore attempts to confirm credentials
        public override Android.OS.Bundle ConfirmCredentials(AccountAuthenticatorResponse response, Account account, Android.OS.Bundle options)
        {
            return null;
        }


        public override Android.OS.Bundle GetAuthToken(AccountAuthenticatorResponse response, Account account, string authTokenType, Android.OS.Bundle options)
        {
            throw new NotImplementedException();
        }

        // Getting a label for the auth token is not supported
        public override string GetAuthTokenLabel(string authTokenType)
        {
            throw new NotImplementedException();
        }

           
        public override Android.OS.Bundle UpdateCredentials(AccountAuthenticatorResponse response, Account account, string authTokenType, Android.OS.Bundle options)
        {
            throw new NotImplementedException();
        }

        // Checking features for the account is not supported
        public override Android.OS.Bundle HasFeatures(AccountAuthenticatorResponse response, Account account, string[] features)
        {
            throw new NotImplementedException();
        }
          
    }
}

