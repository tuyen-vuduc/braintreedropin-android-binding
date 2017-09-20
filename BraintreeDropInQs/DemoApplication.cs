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
using static Java.Lang.Thread;
using BraintreeDropInQs.Internal;
using Square.Retrofit;

namespace BraintreeDropInQs
{
    public class DemoApplication : Application, IUncaughtExceptionHandler
    {
        private static ApiClient sApiClient;

        private Thread.IUncaughtExceptionHandler mDefaultExceptionHandler;


        public override void OnCreate()
        {
            if (BuildConfig.DEBUG)
            {
                StrictMode.SetThreadPolicy(new StrictMode.ThreadPolicy.Builder()
                        .DetectCustomSlowCalls()
                        .DetectNetwork()
                        .PenaltyLog()
                        .PenaltyDeath()
                        .Build());
                StrictMode.SetVmPolicy(new StrictMode.VmPolicy.Builder()
                        .DetectActivityLeaks()
                        .DetectLeakedClosableObjects()
                        .DetectLeakedRegistrationObjects()
                        .DetectLeakedSqlLiteObjects()
                        .PenaltyLog()
                        .PenaltyDeath()
                        .Build());
            }

            base.OnCreate();

            if (Settings.getVersion(this) != BuildConfig.VERSION_CODE)
            {
                Settings.setVersion(this);
                // MailableLog.clearLog(this);
            }
            //MailableLog.init(this, BuildConfig.DEBUG);

            DeveloperTools.setup(this);

            mDefaultExceptionHandler = Thread.DefaultUncaughtExceptionHandler;
            Thread.DefaultUncaughtExceptionHandler = this;
        }


        public void UncaughtException(Thread t, Throwable e)
        {
            // throw log here

        }
        public static ApiClient getApiClient(Context context)
        {
            if (sApiClient == null)
            {
                sApiClient = (ApiClient)new RestAdapter.Builder()
                        .SetEndpoint(Settings.getEnvironmentUrl(context))
                        .SetRequestInterceptor(new ApiClientRequestInterceptor())
                        .Build()
                        .Create(Class.FromType(typeof(ApiClient)));
            }

            return sApiClient;
        }

        public static void resetApiClient()
        {
            sApiClient = null;
        }
    }
}