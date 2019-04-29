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
using Mapsui.Styles;

namespace Android_Mapsui_Vehicle
{
    [Activity(Label = "Vehicle Location", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MapControl mapCtrl;
        private TextView speed, LatLong, Altitude, MaxSpeed;
        private bool AllowMapToLoad = false;
        public static Point Location = new Point(0, 0);
        private MySettings mainsave;
        private ListView ColorList;
        private ArrayAdapter<string> ColorAdapter, MeasurementAdapter;
        private int ListPosition = 0;
        private double Speed, elev;
        private double MaxSpeedFloat = 0;

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
            MaxSpeed = FindViewById<TextView>(Resource.Id.MaxSpeed);

            FirstRun(); // Check if GPS Function enabled

        }

        // Gets called after OnCreate() according to Android Lifecycle. Checks and Starts Location service if allowed
        protected override void OnResume()
        {
            mainsave = MySettings.Load();
            if (AllowMapToLoad == true) // If Location Service is Allowed
            {
                LocService();   // Start Location Service
            }
            System.Threading.Tasks.Task.Run(() =>
            {
                mapCtrl.Map = MapFunctions.CreateMap(ref mainsave); // Create the Map
                mapCtrl.Map.NavigateTo(18);         // Set Inital Zoom Level to be Fairly Close
            });
            if (mainsave.Measurement == MySettings.MeasurementSystem.Imperial)
            {
                MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat + " MPH";
            }
            else
            {

                MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat + " KPH";
            }

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
            catch
            {
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
                Speed = test.Speed * 2.239; // Get the MPH Equivalent of Speed's Meters Per Second
                elev = test.Altitude * 3.28; // Get the Ft Equivalent of Altitude's Meters
                if (Speed > MaxSpeedFloat)
                {
                    MaxSpeedFloat = Speed;
                    MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat.ToString("0.00") + " MPH";
                }
                speed.Text = "Speed: " + Speed.ToString("0.00") + " MPH";
                LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
                Altitude.Text = "Elevation: " + elev.ToString("0.0") + "Ft";

            }
            else
            {
                Speed = test.Speed * 3.6;
                if (Speed > MaxSpeedFloat)
                {
                    MaxSpeedFloat = Speed;
                    MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat.ToString("0.00") + " KPH";
                }
                elev = test.Altitude;
                // Output specific text to the textview
                speed.Text = "Speed: " + Speed.ToString("0.00") + " KPH";
                LatLong.Text = "Latitude: " + test.Latitude + " Longitude: " + test.Longitude;
                Altitude.Text = "Elevation: " + elev.ToString("0.0") + "m";

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
                        // Create a Popup that will allow the user to choose a new color for there Location Dot.
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Change Icon Color");
                        View view = LayoutInflater.Inflate(Resource.Layout.ColorLayout, null);
                        ColorAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, mainsave.ColorsAvailable);

                        
                        ColorList = (ListView)view.FindViewById(Resource.Id.ColorListView);
                        ColorList.Adapter = ColorAdapter;
                        ColorList.ItemClick += colorSelected;

                        ColorList.SetItemChecked(mainsave.IndexColors(), true); // Get the currently selected color and have it selected
                        alert.SetPositiveButton("OK", OkColorAction);   // Assign what happens when the user clicks ok.
                        alert.SetCancelable(false);
                        alert.SetView(view);
                        alert.Show();   // Show Popup
                        return true;
                    }
                case Resource.Id.menuItem3:
                    {
                        // Create popup for if the user wants the units in MPH/Feet or KPH/Meters
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetTitle("Change Measurement System");
                        View view = LayoutInflater.Inflate(Resource.Layout.ColorLayout, null);
                        List<string> temp = new List<string> { "Imperial", "Metric" };
                        MeasurementAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemSingleChoice, temp);

                        // Determine currently selected unit system
                        ColorList = (ListView)view.FindViewById(Resource.Id.ColorListView);
                        ColorList.Adapter = MeasurementAdapter;
                        if (mainsave.Measurement == MySettings.MeasurementSystem.Imperial)
                        {

                            ColorList.SetItemChecked(0, true);
                        }
                        else
                        {
                            ColorList.SetItemChecked(1, true);
                        }

                        
                        ColorList.ItemClick += colorSelected;
                        alert.SetPositiveButton("OK", OkMeasurementAction);
                        alert.SetCancelable(false);
                        alert.SetView(view);
                        alert.Show();
                        return true;
                    }

            }

            return base.OnOptionsItemSelected(item);
        }

        // Called when new color is selected from the list.
        private void colorSelected(object sender, ListView.ItemClickEventArgs e)
        {
            ListPosition = e.Position;
            ColorList.SetItemChecked(e.Position, true);
        }

        // Ok Button action for changing the Units
        private void OkMeasurementAction(object sender, DialogClickEventArgs e)
        {
            if (ListPosition == 0)
            {
                // If switching from metric to Imperial then convert all the text on screen.
                if (mainsave.Measurement == MySettings.MeasurementSystem.Metric)
                {
                    MaxSpeedFloat = MaxSpeedFloat * .621371;
                    Speed = Speed * .621371;
                    MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat.ToString("0.00") + " MPH";
                    speed.Text = "Speed: " + Speed.ToString("0.00") + " MPH";
                    elev = elev * 3.28; // Get the Ft Equivalent of Altitude's Meters
                    Altitude.Text = "Elevation: " + elev.ToString("0.0") + "Ft";
                }
                // Change unit system in the User settings
                mainsave.Measurement = MySettings.MeasurementSystem.Imperial;

            }
            else
            {
                // If switching from imperial to metric then convert all the text on screen
                if (mainsave.Measurement == MySettings.MeasurementSystem.Imperial)
                {
                    MaxSpeedFloat = MaxSpeedFloat * 1.60934;
                    Speed = Speed * 1.60934;
                    MaxSpeed.Text = "Max Speed: " + MaxSpeedFloat.ToString("0.00") + " KPH";
                    speed.Text = "Speed: " + Speed.ToString("0.00") + " KPH";
                    elev = elev * .3048;
                    Altitude.Text = "Elevation: " + elev.ToString("0.0") + "m";
                }
                // Change unit system in user settings
                mainsave.Measurement = MySettings.MeasurementSystem.Metric;
            }
            MySettings.Save(mainsave);
        }
        // Called when the user clicks on OK for the Color selector dialog
        private void OkColorAction(object sender, DialogClickEventArgs e)
        {
            // Change color in user settings
            mainsave.color = mainsave.List2DotColor(ListPosition);

            // Save user settings
            MySettings.Save(mainsave);

            // Change the dot on the map to the new color
            mapCtrl.Map.Layers[1].Style = new SymbolStyle { Fill = { Color = mainsave.ReturnColor() }, SymbolScale = .4 };
        }
    }
}