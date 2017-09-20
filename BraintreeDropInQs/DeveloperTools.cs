﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BraintreeDropInQs
{
    public class DeveloperTools
    {
        public static void setup(Application application)
        {
            LeakLoggerService.setupLeakCanary(application);
            Stetho.initializeWithDefaults(application);
        }
    }
}