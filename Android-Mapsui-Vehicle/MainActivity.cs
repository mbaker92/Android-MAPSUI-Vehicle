﻿using Android.App;
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

namespace Android_Mapsui_Vehicle
{
    [Activity(Label = "Vehicle Location", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MapControl mapCtrl;
        private TextView speed, LatLong, Altitude;
        private bool AllowMapToLoad = false;
        public static Point Location = new Point(0, 0);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            // Stop Location Service if it was started previously (Needed for updating if device is rotated since OnCreate is called again)
            StopService();

            // Set Views of items in activity_main
            mapCtrl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            speed = FindViewById<TextView>(Resource.Id.text1);
            LatLong = FindViewById<TextView>(Resource.Id.text2);
            Altitude = FindViewById<TextView>(Resource.Id.text3);


            FirstRun(); // Check if GPS Function enabled

            mapCtrl.Map = MapFunctions.CreateMap(); // Create the Map

        }

        // Gets called after OnCreate() according to Android Lifecycle. Checks and Starts Location service if allowed
        protected override void OnResume()
        {
            if (AllowMapToLoad == true) // If Location Service is Allowed
            {
                LocService();   // Start Location Service
            }
            base.OnResume();
        }

        // Will Stop location service when user switches to another app. OnResume will be called to restart location service if the navigate back
        protected override void OnPause()
        {
            StopService();
            base.OnPause();
        }

        // functions called when the back button is pressed
        public override void OnBackPressed()
        {
            StopService();  // Stop the Location service if it running
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

            double SpeedtoMPH = test.Speed * 2.239; // Get the MPH Equivalent of Speed's Meters Per Second

            // Output specific text to the textview
            speed.Text = "Speed: " + SpeedtoMPH.ToString() + " MPH";
            LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
            Altitude.Text = "Altitude: " + test.Altitude + "m";

        }

        // Used to start the location service 
        private void LocService()
        {
            mapCtrl.Map.NavigateTo(18);         // Set Inital Zoom Level to be Fairly Close
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STARTING LOCATION SERVICE---------------");
                await StartListening(); // Calls the start listening function
            });
        }

        // Checks if we have GPS permissions 
        private void FirstRun()
        {
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
    }
 }