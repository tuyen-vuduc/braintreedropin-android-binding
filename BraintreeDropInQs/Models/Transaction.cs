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
using GoogleGson.Annotations;

namespace BraintreeDropInQs.Models
{
    public class Transaction: Java.Lang.Object
    {
        [SerializedName(Value = "message")]
        private String mMessage;

        public String getMessage()
        {
            return mMessage;
        }
    }
}