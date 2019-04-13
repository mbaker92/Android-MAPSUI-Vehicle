using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Mapsui.Projection;
using Mapsui.UI.Android;
using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Mapsui.Geometries;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Content;
using Android.Views;
using AlertDialog = Android.App.AlertDialog;
using System.Collections.Generic;

namespace Android_Mapsui_Vehicle
{
    [Activity(Label = "Vehicle Location", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MapControl mapCtrl;
        private TextView speed, LatLong, Altitude;
        private bool AllowMapToLoad = false;
        public static Point Location = new Point(0, 0);
        private MySettings mainsave;
        private ListView ColorList;
        private ArrayAdapter<string> ColorAdapter, MeasurementAdapter;
        private int ListPosition = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            var id = "ca-app-pub-7805135116630476~8762824299";
            MobileAds.Initialize(ApplicationContext, id);
           
            var adView = FindViewById<AdView>(Resource.Id.adView);
            var adRequest = new AdRequest.Builder().AddTestDevice("61BBB1BB35FE0D192638B2A005A2E39A").Build();
           // var adRequest = new AdRequest.Builder().Build();
            adView.LoadAd(adRequest);

            // Stop Location Service if it was started previously (Needed for updating if device is rotated since OnCreate is called again)
            StopService();

            // Set Views of items in activity_main
            mapCtrl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            speed = FindViewById<TextView>(Resource.Id.text1);
            LatLong = FindViewById<TextView>(Resource.Id.text2);
            Altitude = FindViewById<TextView>(Resource.Id.text3);


            FirstRun(); // Check if GPS Function enabled
 //           mainsave = MySettings.Load();

        }

        // Gets called after OnCreate() according to Android Lifecycle. Checks and Starts Location service if allowed
        protected override void OnResume()
        {
            mainsave = MySettings.Load();
            if (AllowMapToLoad == true) // If Location Service is Allowed
            {
                LocService();   // Start Location Service
            }
 
            mapCtrl.Map = MapFunctions.CreateMap(ref mainsave); // Create the Map
            mapCtrl.Map.NavigateTo(18);         // Set Inital Zoom Level to be Fairly Close

            base.OnResume();
        }

        // Will Stop location service when user switches to another app. OnResume will be called to restart location service if the navigate back
        protected override void OnPause()
        {
            StopService();
            MySettings.Save(mainsave);
            base.OnPause();
        }

        // functions called when the back button is pressed
        public override void OnBackPressed()
        {
            StopService();  // Stop the Location service if it running
            MySettings.Save(mainsave);

            base.OnBackPressed();
        }

        // Function used to stop the location service if it is running
        private void StopService()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STOPPING LOCATION SERVICE---------------");
                await StopListening();  // Calls the StopListening function.
            });
        }

        // Used to Stop the location service whenever called so that we save battery life
        async Task StopListening()
        {
            // Try catch block used to turn off location service if it is possible
            try
            {
                await CrossGeolocator.Current.StopListeningAsync(); // Stop location service
            }
            catch {
                // If Location Service is not started
            }
        }

        // StartListening will start the location service so that we can track the Vehicle and show their position on screen
        async Task StartListening()
        {
            System.Diagnostics.Debug.WriteLine("-----------------------IN LISTENER FOR LOCATION-----------"); // Used for Debugging
            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false
            });
            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;     // Go to Current_PositionChanged function to update location point
        }

        // Function to change the Location variable to the Raters current location. Gets the Information whenver position is changed and updates the Values
        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var test = e.Position;  // Get the position
            Location = SphericalMercator.FromLonLat(test.Longitude, test.Latitude); // Store the SphericalMercator coordinates from the Lat and Long of the vehicles position to Location variable

            mapCtrl.Map.NavigateTo(Location);   // Move Screen to Location of Vehicle

            if (mainsave.Measurement == MySettings.MeasurementSystem.Imperial)
            {
                double SpeedtoMPH = test.Speed * 2.239; // Get the MPH Equivalent of Speed's Meters Per Second
                double SeaLevel = test.Altitude * 3.28; // Get the Ft Equivalent of Altitude's Meters

                speed.Text = "Speed: " + SpeedtoMPH.ToString("0.00") + " MPH";
                LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
                Altitude.Text = "Elevation: " + SeaLevel.ToString("0.0") + "Ft";

            }
            else
            {            // Output specific text to the textview
                speed.Text = "Speed: " + test.Speed.ToString("0.00") + " KPH";
                LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
                Altitude.Text = "Elevation: " + test.Altitude.ToString("0.0") + "m";

            }

        }

        // Used to start the location service 
        private void LocService()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STARTING LOCATION SERVICE---------------");
                await StartListening(); // Calls the start listening function
            });
        }

        // Checks if we have GPS permissions 
        private void FirstRun()
        {
            const string PREFS_NAME = "PrefsFile";
            const string PrefVersionCodeKey = "version_code";
            int DoesntExist = -1;

            // Get Current Version of Application
            int currentVersionCode = Application.Context.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionCode;


            ISharedPreferences prefs = GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            int savedVersionCode = prefs.GetInt(PrefVersionCodeKey, DoesntExist);

            // If current version then its a normal run.
            if (currentVersionCode == savedVersionCode)
            {
                mainsave = MySettings.Load();
            }
            else if (savedVersionCode == DoesntExist)
            {
                // New Install or User Cleared Shared Preferences
                mainsave = new MySettings();
                System.Diagnostics.Debug.WriteLine("Should Have Created a new Mainsave");
                MySettings.Save(mainsave);

            }
            else if (currentVersionCode > savedVersionCode)
            {
                mainsave = MySettings.Load();
                // Upgrade of Application
            }

            // Update shared preferences with current version code
            prefs.Edit().PutInt(PrefVersionCodeKey, currentVersionCode).Apply();



            if (CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    RequestPermissions(new string[] { Android.Manifest.Permission.AccessFineLocation }, 1);
                }).ConfigureAwait(true);
                speed.Text = "Location Service Needs to Be Allowed";
            }
            else
            {
                AllowMapToLoad = true;
            }
        }


        // Function to get the results of from the CheckSelfPermission function in the FirstRun function
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {





        if (permissions[0] == "android.permission.ACCESS_FINE_LOCATION")
            {
                if (grantResults[0] == Permission.Granted)
                {
                    // Allow Map to Load
                    AllowMapToLoad = true;
                    LocService();
                }
            }
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // set the menu layout on Main Activity  
            MenuInflater.Inflate(Resource.Menu.TpMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menuItem1:
                    {
                        // add your code  
                        return true;
                    }
                case Resource.Id.menuItem2:
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Change Icon Color");
                        View view = LayoutInflater.Inflate(Resource.Layout.ColorLayout, null);
                        ColorAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice,mainsave.ColorsAvailable);
                      
                        ColorList = (ListView)view.FindViewById(Resource.Id.ColorListView);
                        ColorList.Adapter = ColorAdapter;
                        ColorList.ItemClick += colorSelected;

                        ColorList.SetItemChecked(mainsave.IndexColors(), true);
                        alert.SetPositiveButton("OK", OkColorAction);
                        alert.SetView(view);
                        alert.Show();
                        return true;
                    }
                case Resource.Id.menuItem3:
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Change Measurement System");
                        View view = LayoutInflater.Inflate(Resource.Layout.ColorLayout, null);
                        List<string> temp = new List<string> { "Imperial", "Metric" };
                        MeasurementAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, temp);

                        ColorList = (ListView)view.FindViewById(Resource.Id.ColorListView);
                        ColorList.Adapter = MeasurementAdapter;
                        if(mainsave.Measurement == MySettings.MeasurementSystem.Imperial)
                        {

                            ColorList.SetItemChecked(0, true);
                        }
                        else
                        {
                            ColorList.SetItemChecked(1, true);
                        }
                        ColorList.ItemClick += colorSelected;
                        alert.SetPositiveButton("OK", OkMeasurementAction);
                        alert.SetView(view);
                        alert.Show();
                        //    MySettings.Save(mainsave);
                        //  Intent refresh = new Intent(this, typeof(MainActivity));
                        //StartActivity(refresh);
                        // Finish();
                        // add your code  
                        return true;
                    }

            }

            return base.OnOptionsItemSelected(item);
        }

        private void colorSelected(object sender, ListView.ItemClickEventArgs e)
        {
            ListPosition = e.Position;
            ColorList.SetItemChecked(e.Position, true);
        }

        private void OkMeasurementAction(object sender, DialogClickEventArgs e)
        {
            if(ListPosition == 0)
            {
                mainsave.Measurement = MySettings.MeasurementSystem.Imperial;

            }
            else
            {
                mainsave.Measurement = MySettings.MeasurementSystem.Metric;
            }
            MySettings.Save(mainsave);
        }

            private void OkColorAction(object sender, DialogClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Value of ListPosition " + ListPosition);
            mainsave.color = mainsave.List2DotColor(ListPosition);
            MySettings.Save(mainsave);
            Intent refresh = new Intent(this, typeof(MainActivity));
            StartActivity(refresh);
            Finish();
        }

        //   private void ColorSpin(ref View view)
        // {
        //       ColorSpinner = (Spinner)view.FindViewById(Resource.Id.ColorSpinner);
        //}



    }
 }