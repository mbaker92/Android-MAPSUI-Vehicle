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
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Android_Mapsui_Vehicle
{
    class MapFunctions
    {
        public static Map CreateMap()
        {
            Map map = new Map();                                                                    // Declare the map variable
            map.Layers.Add(OpenStreetMap.CreateTileLayer());                                                            // Create map after determining if there is an offline or online one available
            map.Layers.Add(new AnimatedPointsWithAutoUpdateLayer { Name = "Rater Location" });      // Create the Layer for the Raters Location and add it Layers
            return map;                                                                             // Return the map

        }
    }
    // Classes used to get the position of the Vehicle to show up on the map.
    public class AnimatedPointsWithAutoUpdateLayer : AnimatedPointLayer
    {
        private readonly System.Threading.Timer _timer; // timer used in polling the update data

        // Create a Layer that will update the position of the rater using a DynamicMemoryProvider
        public AnimatedPointsWithAutoUpdateLayer()
            : base(new DynamicMemoryProvider())
        {
            Style = new SymbolStyle { Fill = { Color = new Color(107, 244, 66, 255) }, SymbolScale = .5 };  // Set the symbol color and size used to represent the Vehicle on the map
            _timer = new System.Threading.Timer(arg => UpdateData(), this, 0, 3000);                        // Set the timer to update the data from DynamicMemoryProvider

        }

        // Class used for the position of the rater. 
        private class DynamicMemoryProvider : MemoryProvider
        {
            // Function used to update the raters position. Called each time there is and Update from UpdateData
            public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
            {

                var features = new List<IFeature>();    // Create a blank feature list
                var feature = new Feature();            // Create a blank feature
                feature.Geometry = MainActivity.Location; // Set the features geometry as the point of where the rater is
                feature["ID"] = "Location of Vehicle";      // Set an Identifier
                features.Add(feature);                  // Add feature to the feature list
                return features;                        // return the feature list
            }
        }
    }
}