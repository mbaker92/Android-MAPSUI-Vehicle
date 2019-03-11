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

namespace Android_Mapsui_Vehicle
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MapControl mapCtrl;
        public static Point Location = new Point(0, 0);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            System.Threading.Tasks.Task.Run(async () =>
            {
                System.Diagnostics.Debug.WriteLine("-------------STARTING LOCATION SERVICE---------------");
                await StartListening();
            });
        }       
        
        // Used to Stop the location service whenever called so that we save battery life
        async Task StopListening()
        {
            await CrossGeolocator.Current.StopListeningAsync(); // Stop location service
        }
        // StartListening will start the location service so that we can track the rater and show their position on screen
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

        // Function to change the Location variable to the Raters current location. Gets the Information whenver position is changed
        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var test = e.Position;  // Get the position
            Location = SphericalMercator.FromLonLat(test.Longitude, test.Latitude); // Store the SphericalMercator coordinates from the Lat and Long of the raters position to Location variable
        }
    }
}