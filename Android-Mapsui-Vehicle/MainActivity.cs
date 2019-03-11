using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Projection;
using Mapsui.Layers;
using Mapsui;
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

            // Set Views of items in activity_main
            mapCtrl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            speed = FindViewById<TextView>(Resource.Id.text1);
            LatLong = FindViewById<TextView>(Resource.Id.text2);
            Altitude = FindViewById<TextView>(Resource.Id.text3);

            FirstRun();

            mapCtrl.Map = MapFunctions.CreateMap();
            if (AllowMapToLoad == true)
            {
                LocService();
            }
        }
        public override void OnBackPressed()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STOPPING LOCATION SERVICE---------------");
                await StopListening();
            });
            base.OnBackPressed();
        }

        // Used to Stop the location service whenever called so that we save battery life
        async Task StopListening()
        {
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

            speed.Text = "Speed: " + SpeedtoMPH.ToString() + " MPH";
            LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
            Altitude.Text = "Altitude: " + test.Altitude + "m";

        }
        private void LocService()
        {
            mapCtrl.Map.NavigateTo(18);         // Set Inital Zoom Level to be Fairly Close
            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STARTING LOCATION SERVICE---------------");
                await StartListening();
            });
        }
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