using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Android.Preferences;

namespace BraintreeDropInQs.Internal
{
    public class LogReporting
    {
        private static Context mContext;
        private ProgressDialog mLoading;
        private static Intent mEmailIntent;

        public LogReporting(Context context)
        {
            mContext = context;
        }

        public void collectAndSendLogs()
        {
            mLoading = ProgressDialog.Show(mContext, "", mContext.GetString(Resource.String.loading), true);
            new GenerateLogFile().Execute();
        }


        private class GenerateLogFile : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(mContext);
              Java.Lang.StringBuilder message = new Java.Lang.StringBuilder();

                message.Append("Android version: " + Build.VERSION.SdkInt + "\n");
                message.Append("Device manufacturer: " + Build.Manufacturer + "\n");
                message.Append("Device model: " + Build.Model + "\n");
                message.Append("Device product: " + Build.Product + "\n");
                message.Append("App version: " + BuildConfig.VERSION_NAME + "\n");
                message.Append("Debug: " + BuildConfig.DEBUG + "\n");

                Dictionary<string, object> keys = new Dictionary<string, object>(prefs.All);
                foreach (var key in keys.ToList())
                {
                    message.Append(key.Key + ": " + key.Value.ToString() + "\n");
                }
                message.Append("---------------------------");
                message.Append("\n");

                //try
                //{
                //    mEmailIntent = MailableLog.buildEmailIntent(mContext,
                //            "team-android@getbraintree.com", "Braintree Android Demo App Debug Log",
                //            "braintree-android.log", message.ToString());
                //}
                //catch (IOException e)
                //{
                //    LoggerFactory.getLogger("LogReporting").error(e.getMessage());
                //}

                // Ensure we show the spinner and don't just flash the screen
                SystemClock.Sleep(1000);

                return null;
            }
        }


    }
}