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
using System.Threading.Tasks;

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


        private async void sendNonceToServer(PaymentMethodNonce nonce)
        {
            Task<Transaction> task;

            if (Settings.isThreeDSecureEnabled(this) && Settings.isThreeDSecureRequired(this))
            {
                task = DemoApplication.getApiClient(this).CreateTransaction(nonce.Nonce, Settings.getThreeDSecureMerchantAccountId(this), true);
            }
            else if (Settings.isThreeDSecureEnabled(this))
            {
                task = DemoApplication.getApiClient(this).CreateTransaction(nonce.Nonce, Settings.getThreeDSecureMerchantAccountId(this));
            }
            else if (nonce is CardNonce && ((CardNonce)nonce).CardType.Equals("UnionPay"))
            {
                task = DemoApplication.getApiClient(this).CreateTransaction(nonce.Nonce, Settings.getUnionPayMerchantAccountId(this));
            }
            else
            {
                task = DemoApplication.getApiClient(this).CreateTransaction(nonce.Nonce, Settings.getMerchantAccountId(this));
            }

            var transaction = await task;

            if (transaction == null)
            {
                setStatus(Resource.String.transaction_failed);
                setMessage(new Java.Lang.String("Unable to create a transaction"));

                return;
            }

            if (transaction.Message != null &&
                   transaction.Message.StartsWith("created"))
            {
                setStatus(Resource.String.transaction_complete);
                setMessage(new Java.Lang.String(transaction.Message));
            }
            else
            {
                setStatus(Resource.String.transaction_failed);
                if (TextUtils.IsEmpty(transaction.Message))
                {
                    setMessage(new Java.Lang.String("Server response was empty or malformed"));
                }
                else
                {
                    setMessage(new Java.Lang.String(transaction.Message));
                }
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
}