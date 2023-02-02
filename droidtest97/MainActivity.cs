using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using AndroidX.Activity.Result;
using Android.Content;
using AndroidX.Activity.Result.Contract;

using Com.Gowtham.Library.Utils;
using Java.Lang;
using Android.Util;


namespace droidtest97
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ActivityResultLauncher activityResultLauncher;
        private ActivityResultLauncher activityResultLauncher2;
        private static string TAG = "MainActivity";
   
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            ActivityResultCallback activityResultCallback = new ActivityResultCallback();
            activityResultCallback.OnActivityResultCalled += ActivityResultCallback_ActivityResultCalled;

            activityResultLauncher = RegisterForActivityResult(new ActivityResultContracts.StartActivityForResult(), activityResultCallback);


            ActivityResultCallback activityResultCallback2 = new ActivityResultCallback();
            activityResultCallback2.OnActivityResultCalled += ActivityResultCallback_ActivityResultCalled2;

            activityResultLauncher2 = RegisterForActivityResult(new ActivityResultContracts.StartActivityForResult(), activityResultCallback);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;
        }

        /*
        public class ActivityResultCallback2 : Java.Lang.Object, IActivityResultCallback
        {
            public EventHandler<ActivityResult> OnActivityResultCalled;
            public EventHandler<Intent> intent;

            public void OnActivityResult(Java.Lang.Object result)
            {
                ActivityResult activityResult = result as ActivityResult;
                OnActivityResultCalled?.Invoke(this, activityResult);
            }

            public void OnIntent(Java.Lang.Object finish)
            {
                Intent _intent = finish as Intent;
                intent?.Invoke(this, _intent);
            }
        }
        */
        public class ActivityResultCallback : Java.Lang.Object, IActivityResultCallback
        {
            public EventHandler<ActivityResult> OnActivityResultCalled;

            public void OnActivityResult(Java.Lang.Object result)
            {
                ActivityResult activityResult = result as ActivityResult;
                OnActivityResultCalled?.Invoke(this, activityResult);
            }
        }

        private void ActivityResultCallback_ActivityResultCalled(object sender, ActivityResult result)
        {
            if (result.ResultCode == (int)Result.Ok && result.Data != null)
            {
                Intent data = result.Data;
                if (data.Data != null)
                {
                  //  LogMessage.V("Video path:: " + data.Data);
                   openTrimActivity((string)(data.Data));
                }
            }

        }


        private void ActivityResultCallback_ActivityResultCalled2(object sender, ActivityResult result)
        {
            Intent intent = new Intent();
            if (result.ResultCode == (int)Result.Ok && result.Data != null)
            {

                Android.Net.Uri uri = Android.Net.Uri.Parse(TrimVideo.GetTrimmedVideoPath(result.Data));
                Log.Debug(TAG, "Video size:: " + uri);
            }

        }

        private void openTrimActivity(string data)
        {
            Context context = Android.App.Application.Context;
            
            TrimVideo.Activity(data)


                   .SetTrimType(TrimType.FixedDuration)
                   .SetFixedDuration(57)
                   //.SetTrimType(TrimType.MinMaxDuration)
                  // .SetMinToMax(30,60)
                   .SetAccurateCut(false)
                   .Start(this, activityResultLauncher2)
                  ;

            //startActivity(TrimVideo.Activity(data), activityResultLauncher2);

        }

  
        private void startActivity(Activity activity, ActivityResultLauncher launcher)
        {
            // validate();
           Intent intent = new Intent();
          
        }

       

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            Intent intent = new Intent();
            intent.SetType("video/*");
            intent.SetAction(Intent.ActionGetContent);
            activityResultLauncher.Launch(Intent.CreateChooser(intent, "Select Video"));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
