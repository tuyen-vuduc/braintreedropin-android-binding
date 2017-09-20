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
using Google.Gson.Annotations;

namespace BraintreeDropInQs.Models
{
    public class ClientToken
    {
        [SerializedName(Value = "client_token")]
        private String mClientToken;

        public String getClientToken()
        {
            return mClientToken;
        }
    }
}