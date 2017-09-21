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
using static Android.Provider.CalendarContract;
using BraintreeDropInQs.Models;
using Square.Retrofit;
using Square.Retrofit.Client;
using BraintreeDropInQs.Internal;

namespace BraintreeDropInQs
{
    [Activity(Label = "BaseActivity")]
    public abstract class BaseActivity : AppCompatActivity, IOnRequestPermissionsResultCallback, IPaymentMethodNonceCreatedListener, IBraintreeCancelListener, IBraintreeErrorListener, Android.Support.V7.App.ActionBar.IOnNavigationListener, IDialogInterfaceOnClickListener
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

            if (savedInstanceState != null && savedInstanceState.ContainsKey(KEY_AUTHORIZATION))
            {
                mAuthorization = savedInstanceState.GetString(KEY_AUTHORIZATION);
            }
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

        public  override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
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
                        .FindFragmentByTag("com.braintreepayments.api.BraintreeFragment");// change this line later
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
        protected abstract void reset();
        protected abstract void onAuthorizationFetched();
        protected void fetchAuthorization()
        {
            if (mAuthorization != null)
            {
                onAuthorizationFetched();
            }
            else if (Settings.useTokenizationKey(this))
            {
                mAuthorization = Settings.getEnvironmentTokenizationKey(this);
                onAuthorizationFetched();
            }
            else
            {
                DemoApplication.getApiClient(this).GetClientToken(
                        Settings.getCustomerId(this),
                        Settings.getMerchantAccountId(this))
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted || TextUtils.IsEmpty(t.Result))
                            {
                                showDialog("Client token was empty");
                            }
                            else
                            {
                                mAuthorization = t.Result;
                                onAuthorizationFetched();
                            }
                        });
            }
        }



        private void setupActionBar()
        {
            Android.Support.V7.App.ActionBar actionBar = SupportActionBar;
            actionBar.SetDisplayShowTitleEnabled(false);
            actionBar.NavigationMode = (int)ActionBarNavigationMode.List;

            ArrayAdapter adapter =ArrayAdapter.CreateFromResource(this,
                    Resource.Array.environments, Android.Resource.Layout.SimpleDropDownItem1Line);
            actionBar.SetListNavigationCallbacks(adapter, this);
            actionBar.SetSelectedNavigationItem(Settings.getEnvironment(this));
        }


        public virtual void OnCancel(int p0)
        {
            //mLogger.debug("Cancel received: " + requestCode);
        }

        public virtual void OnError(Java.Lang.Exception p0)
        {
            //mLogger.debug("Error received (" + error.getClass() + "): " + error.getMessage());
            //mLogger.debug(error.toString());

            showDialog("An error occurred ");
        }

        public bool OnNavigationItemSelected(int itemPosition, long itemId)
        {
            if (Settings.getEnvironment(this) != itemPosition)
            {
                Settings.setEnvironment(this, itemPosition);
                performReset();
            }
            return true;

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return true;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.reset:
                    performReset();
                    return true;
                case Resource.Id.settings:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    return true;
                default:
                    return false;
            }
        }

        public virtual  void OnPaymentMethodNonceCreated(PaymentMethodNonce p0)
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


    public class MyCallBack : Java.Lang.Object, Square.Retrofit.ICallback
    {

        public Action<RetrofitError> MFailure;
        public Action<Models.ClientToken, Response> MSuccess;


        public void Failure(RetrofitError p0)
        {
            MFailure?.Invoke(p0);
        }

        public void Success(Java.Lang.Object p0, Response p1)
        {
            MSuccess?.Invoke((Models.ClientToken)p0, p1);


        }
    }
}
