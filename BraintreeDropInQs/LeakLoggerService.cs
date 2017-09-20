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
using Square.LeakCanary;

namespace BraintreeDropInQs
{
    public class LeakLoggerService : DisplayLeakService
    {
        public static void setupLeakCanary(Application application)
        {
             LeakCanaryXamarin.Install(application);
    }

        protected override void AfterDefaultHandling(HeapDump heapDump, AnalysisResult result, string leakInfo)
        {
            if (!result.LeakFound || result.ExcludedLeak)
            {
                return;
            }

            
            //LoggerFactory.getLogger("LeakCanary").warn(leakInfo);
        }


    }
}