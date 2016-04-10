using System;
using Android.Content;
using Android.OS;
using Android.Accounts;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Sunshine.Sync
{
    public class SunshineSyncAdapter : AbstractThreadedSyncAdapter
    {
        readonly ILogger _log;


        const string LogTag = "SunshineSyncAdapter";

        public SunshineSyncAdapter(Context context, bool autoInitialize)
            : base(context, autoInitialize)
        {
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = LogEventLevel.Verbose;
            _log = new LoggerConfiguration().MinimumLevel.ControlledBy(levelSwitch).WriteTo.AndroidLog().CreateLogger();
        
        }

        public override void OnPerformSync(Android.Accounts.Account account, Android.OS.Bundle extras, string authority, ContentProviderClient provider, SyncResult syncResult)
        {
            _log.ForContext<SunshineSyncAdapter>().Debug("onPerformSync Called.");
        }

        /// <summary>
        /// Helper method to have the sync adapter sync immediately
        /// </summary>
        /// <param name="context">The context used to access the account service</param>
        public static void SyncImmediately(Context context)
        {
            var bundle = new Bundle();
            bundle.PutBoolean(ContentResolver.SyncExtrasExpedited, true);
            bundle.PutBoolean(ContentResolver.SyncExtrasManual, true);
            ContentResolver.RequestSync(GetSyncAccount(context),
                context.GetString(Resource.String.content_authority), bundle);
        }

        /// <summary>
        /// Helper method to get the fake account to be used with SyncAdapter, or make a new one
        /// if the fake account doesn't exist yet.  If we make a new account, we call the
        /// onAccountCreated method so we can initialize things.
        /// </summary>
        /// <returns>A fake account.</returns>
        /// <param name="context">The context used to access the account service</param>
        public static Account GetSyncAccount(Context context)
        {
            // Get an instance of the Android account manager
            var accountManager =
                (AccountManager)context.GetSystemService(Context.AccountService);

            // Create the account type and default account
            var newAccount = new Account(
                                 context.GetString(Resource.String.app_name), context.GetString(Resource.String.sync_account_type));

            // If the password doesn't exist, the account doesn't exist
            if (null == accountManager.GetPassword(newAccount))
            {

                // Add the account and account type, no password or user data
                // If successful, return the Account object, otherwise report an error.
                if (!accountManager.AddAccountExplicitly(newAccount, "", null))
                {
                    return null;
                }
                    
                // If you don't set android:syncable="true" in
                // in your <provider> element in the manifest,
                // then call ContentResolver.setIsSyncable(account, AUTHORITY, 1)
                //here.
            }
            return newAccount;
        }
    }
}

