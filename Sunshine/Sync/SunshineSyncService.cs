using System;
using Android;
using Android.App;


namespace Sunshine.Sync
{

    [Service(Exported = true)]
    [IntentFilter(new [] { "android.content.SyncAdapter" })]
    [MetaData("android.content.SyncAdapter", Resource = "@xml/syncadapter")]
    public class SunshineSyncService : Android.App.Service
    {

        private static object syncAdapterLock = new Object();
        private static SunshineSyncAdapter sunshineSyncAdapter = null;

        public override void OnCreate()
        {
            base.OnCreate();

            lock (syncAdapterLock)
            {
                if (sunshineSyncAdapter == null)
                {
                    sunshineSyncAdapter = new SunshineSyncAdapter(ApplicationContext, true);
                }
                
            }

        }


        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
            return sunshineSyncAdapter.SyncAdapterBinder;

        }



    }
}

