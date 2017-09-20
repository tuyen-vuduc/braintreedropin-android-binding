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
using Square.Retrofit.Http;
using Square.Retrofit.Client;
using Square.Retrofit;

namespace BraintreeDropInQs.Internal
{
    public interface ApiClient 
    {
     [GET(Value = "/client_token")]
    void getClientToken([Query(Value ="customer_id")] String customerId, [Query(Value ="merchant_account_id")] String merchantAccountId, Square.Retrofit.ICallback callback);

    [FormUrlEncoded]
    [POST(Value ="/nonce/transaction")]
    void createTransaction([Field(Value ="nonce")] String nonce, Square.Retrofit.ICallback callback);

    [FormUrlEncoded]
    [POST(Value ="/nonce/transaction")]
    void createTransaction([Field(Value ="nonce")] String nonce, [Field(Value ="merchant_account_id")] String merchantAccountId, Square.Retrofit.ICallback callback);

    [FormUrlEncoded]
    [POST(Value ="/nonce/transaction")]
    void createTransaction([Field(Value ="nonce")] String nonce, [Field(Value ="merchant_account_id")] String merchantAccountId, [Field(Value ="three_d_secure_required")] bool requireThreeDSecure, Square.Retrofit.ICallback callback);
 
    }
    public class NXApiClient : Java.Lang.Object, ApiClient
    {
        public void createTransaction([Field(Value = "nonce")] string nonce, ICallback callback)
        { 
        }

        public  void createTransaction([Field(Value = "nonce")] string nonce, [Field(Value = "merchant_account_id")] string merchantAccountId, ICallback callback)
        {
             
        }

        public void createTransaction([Field(Value = "nonce")] string nonce, [Field(Value = "merchant_account_id")] string merchantAccountId, [Field(Value = "three_d_secure_required")] bool requireThreeDSecure, ICallback callback)
        {
             
        }

        public void getClientToken([Query(Value = "customer_id")] string customerId, [Query(Value = "merchant_account_id")] string merchantAccountId, ICallback callback)
        {
             
        }
    }
}