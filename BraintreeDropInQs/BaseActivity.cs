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
using static Android.Support.V4.App.ActivityCompat;
using Com.Braintreepayments.Api.Interfaces;
using Com.Braintreepayments.Api.Models;
using Java.Lang;
using Com.Braintreepayments.Api;
using BraintreeDropInQs.ApiInternal;
using Com.Paypal.Android.Sdk.Onetouch.Core;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Text;

namespace BraintreeDropInQs
{
    [Activity(Label = "BaseActivity")]
    public class BaseActivity : AppCompatActivity, IOnRequestPermissionsResultCallback, IPaymentMethodNonceCreatedListener, IBraintreeCancelListener, IBraintreeErrorListener, Android.Support.V7.App.ActionBar.IOnNavigationListener, IDialogInterfaceOnClickListener
    {
        public static string WRITE_EXTERNAL_STORAGE = "android.permission.WRITE_EXTERNAL_STORAGE";
        private static string KEY_AUTHORIZATION = "com.braintreepayments.demo.KEY_AUTHORIZATION";


        private Android.App.AlertDialog dialog;
        protected string mAuthorization;
        protected string mCustomerId;
        protected BraintreeFragment mBraintreeFragment;
        // protected Logger mLogger;

        private bool mActionBarSetup;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!mActionBarSetup)
            {
                setupActionBar();
                mActionBarSetup = true;
            }

            SignatureVerificationOverrides.disableAppSwitchSignatureVerification(
                    Settings.isPayPalSignatureVerificationDisabled(this));
            PayPalOneTouchCore.UseHardcodedConfig(this, Settings.useHardcodedPayPalConfiguration(this));

            if (BuildConfig.DEBUG && ContextCompat.CheckSelfPermission(this, WRITE_EXTERNAL_STORAGE) != 0)
            {
                ActivityCompat.RequestPermissions(this, new string[] { (string)WRITE_EXTERNAL_STORAGE }, 1);
            }
            else
            {
                handleAuthorizationState();
            }
        }




        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            handleAuthorizationState();


        }



        private void handleAuthorizationState()
        {
            if (mAuthorization == null ||
                    (Settings.useTokenizationKey(this) && !mAuthorization.Equals(Settings.getEnvironmentTokenizationKey(this))) ||
                    !TextUtils.Equals(mCustomerId, Settings.getCustomerId(this)))
            {
                performReset();
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (mAuthorization != null)
            {
                outState.PutString(KEY_AUTHORIZATION, mAuthorization);
            }
        }




        private void performReset()
        {
            mAuthorization = null;
            mCustomerId = Settings.getCustomerId(this);

            if (mBraintreeFragment == null)
            {
                mBraintreeFragment = (BraintreeFragment)FragmentManager
                        .FindFragmentByTag("Naxam");
            }

            if (mBraintreeFragment != null)
            {
                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.N)
                {
                    FragmentManager.BeginTransaction().Remove(mBraintreeFragment).CommitNow();
                }
                else
                {
                    FragmentManager.BeginTransaction().Remove(mBraintreeFragment).Commit();
                    FragmentManager.ExecutePendingTransactions();
                }

                mBraintreeFragment = null;
            }

            reset();
            fetchAuthorization();
        }



        private void setupActionBar()
        {
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetDisplayShowTitleEnabled(false);
            actionBar.NavigationMode = (int)ActionBarNavigationMode.List;

            ArrayAdapter<CharSequence> adapter = ArrayAdapter.CreateFromResource(this,
                    Resource.Array.environments, Android.Resource.Layout.SimpleDropDownItem1Line);
            actionBar.SetListNavigationCallbacks(adapter, this);
            actionBar.SetSelectedNavigationItem(Settings.getEnvironment(this));
        }


        public void OnCancel(int p0)
        {
            //mLogger.debug("Cancel received: " + requestCode);
        }

        public void OnError(Java.Lang.Exception p0)
        {
            //mLogger.debug("Error received (" + error.getClass() + "): " + error.getMessage());
            //mLogger.debug(error.toString());

            showDialog("An error occurred ");
        }

        public bool OnNavigationItemSelected(int itemPosition, long itemId)
        {
            throw new NotImplementedException();
        }

        public void OnPaymentMethodNonceCreated(PaymentMethodNonce p0)
        {
            //mLogger.debug("Payment Method Nonce received: " + paymentMethodNonce.getTypeLabel());
        }

        protected void showDialog(string message)
        {
            dialog = new Android.App.AlertDialog.Builder(this)
                    .SetMessage(message)
                    .SetPositiveButton(Android.Resource.String.Ok, this)
                    .Show();
        }

        protected void setUpAsBack()
        {
            if (ActionBar != null)
            {
                ActionBar.SetDisplayHomeAsUpEnabled(true);
            }
        }



        public void OnClick(IDialogInterface dialog, int which)
        {
            dialog.Dismiss();
        }
    }
}
