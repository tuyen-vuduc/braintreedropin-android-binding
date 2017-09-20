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
using Android.Support.V7.App;
using Java.Lang;
using Square.Retrofit;
using Square.Retrofit.Client;
using Com.Braintreepayments.Api.Models;
using BraintreeDropInQs.Models;
using Android.Text;

namespace BraintreeDropInQs
{
    [Activity(Label = "CreateTransactionActivity")]
    public class CreateTransactionActivity : AppCompatActivity
    {
        public static Java.Lang.String EXTRA_PAYMENT_METHOD_NONCE = new Java.Lang.String("nonce");

        private ProgressBar mLoadingSpinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.create_transaction_activity);
            mLoadingSpinner = FindViewById<ProgressBar>(Resource.Id.loading_spinner);
            SetTitle(Resource.String.processing_transaction);
            sendNonceToServer((PaymentMethodNonce)Intent.GetParcelableExtra(EXTRA_PAYMENT_METHOD_NONCE.ToString()));
        }


        private void sendNonceToServer(PaymentMethodNonce nonce)
        {
            MCallback callback = new MCallback
            {
                MSuccess = (transaction, response) =>
                {
                    if (transaction.getMessage() != null &&
                       transaction.getMessage().StartsWith("created"))
                    {
                        setStatus(Resource.String.transaction_complete);
                        setMessage(new Java.Lang.String(transaction.getMessage()));
                    }
                    else
                    {
                        setStatus(Resource.String.transaction_failed);
                        if (TextUtils.IsEmpty(transaction.getMessage()))
                        {
                            setMessage(new Java.Lang.String("Server response was empty or malformed"));
                        }
                        else
                        {
                            setMessage(new Java.Lang.String(transaction.getMessage()));
                        }
                    }
                }
                ,

                MFailure = (error) =>
                 {
                     setStatus(Resource.String.transaction_failed);
                     setMessage(new Java.Lang.String("Unable to create a transaction. Response Code: " +
                            error.Response.Status + " Response body: " +
                            error.Response.Body));

                 }



            };
            if (Settings.isThreeDSecureEnabled(this) && Settings.isThreeDSecureRequired(this))
            {
                DemoApplication.getApiClient(this).createTransaction(nonce.Nonce,
                        Settings.getThreeDSecureMerchantAccountId(this), true, callback);
            }
            else if (Settings.isThreeDSecureEnabled(this))
            {
                DemoApplication.getApiClient(this).createTransaction(nonce.Nonce,
                        Settings.getThreeDSecureMerchantAccountId(this), callback);
            }
            else if (nonce is CardNonce && ((CardNonce)nonce).CardType.Equals("UnionPay")) {
                DemoApplication.getApiClient(this).createTransaction(nonce.Nonce,
                        Settings.getUnionPayMerchantAccountId(this), callback);
            } else {
                DemoApplication.getApiClient(this).createTransaction(nonce.Nonce, Settings.getMerchantAccountId(this),
                        callback);
            } 
        }


        private void setStatus(int message)
        {
            mLoadingSpinner.Visibility = ViewStates.Gone;
            SetTitle(message);
            TextView status = FindViewById<TextView>(Resource.Id.transaction_status);
            status.SetText(message);
            status.Visibility = ViewStates.Gone;
        }

        private void setMessage(Java.Lang.String message)
        {
            mLoadingSpinner.Visibility = ViewStates.Gone;
            TextView textView = FindViewById<TextView>(Resource.Id.transaction_message);
            textView.Text = message.ToString();
            textView.Visibility = ViewStates.Visible;
        }
    }

    public class MCallback : Java.Lang.Object, Square.Retrofit.ICallback
    {

        public Action<RetrofitError> MFailure;
        public Action<Transaction, Response> MSuccess;

        public void Failure(RetrofitError p0)
        {
            MFailure?.Invoke(p0);
        }

        public void Success(Java.Lang.Object p0, Response p1)
        {

            MSuccess?.Invoke((Transaction)p0, p1);
        }
    }
}