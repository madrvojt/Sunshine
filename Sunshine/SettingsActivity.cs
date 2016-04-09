using System;
using Android.Preferences;
using Android.App;
using Android.Annotation;

namespace Sunshine
{
    [Activity(Label = "@string/title_activity_settings", Theme = "@style/SettingsTheme", Icon = "@mipmap/ic_launcher", ParentActivity = typeof(SettingsActivity))]            
    [MetaData("android.support.PARENT_ACTIVITY", Value = "cz.madrvojt.xamarin.sunshine.SettingActivity")]
    public class SettingsActivity : PreferenceActivity
    {
      
        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Obsolete, because we want start app in old devices pre Honeycomb
            AddPreferencesFromResource(Resource.Xml.pref_general);
            BindPreferenceSummaryToValue(FindPreference(GetString(Resource.String.pref_location_key)));
            BindPreferenceSummaryToValue(FindPreference(GetString(Resource.String.pref_units_key)));
        }

        /// <summary>
        /// Attaches a listener so the summary is always updated with the preference value.
        /// Also fires the listener once, to initialize the summary (so it shows up before the value
        /// is changed.
        /// </summary>
        void BindPreferenceSummaryToValue(Preference preference)
        {
            // Set the listener to watch for value changes.
            preference.PreferenceChange += (sender, e) => OnPreferenceChange(e.Preference, e.NewValue);

            // Trigger the listener immediately with the preference's current value.
            OnPreferenceChange(preference,
                PreferenceManager
                .GetDefaultSharedPreferences(preference.Context)
                .GetString(preference.Key, ""));
        }

        public override Android.Content.Intent ParentActivityIntent
        {
            get
            {
                var intent = base.ParentActivityIntent;
                intent.SetFlags(Android.Content.ActivityFlags.ClearTop);
                return intent;
            }
        }

        /// <summary>
        /// Raises the preference change event.
        /// </summary>
        void OnPreferenceChange(Preference preference, Object value)
        {
            string stringValue = value.ToString();

            var listPreference = preference as ListPreference;
            if (listPreference != null)
            {
                // For list preferences, look up the correct display value in
                // the preference's 'entries' list (since they have separate labels/values).
                int prefIndex = listPreference.FindIndexOfValue(stringValue);
                if (prefIndex >= 0)
                {
                    preference.Summary = listPreference.GetEntries()[prefIndex];
                }
            }
            else
            {
                // For other preferences, set the summary to the value's simple string representation.
                preference.Summary = stringValue;
            }
        }



    }
}

