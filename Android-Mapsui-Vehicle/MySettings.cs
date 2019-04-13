using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mapsui.Styles;

namespace Android_Mapsui_Vehicle
{
    [Serializable]

    public class MySettings
    {

        public enum MeasurementSystem { Metric, Imperial};
        public enum DotColor { Red, Green, Blue, Yellow, Purple, Pink, Orange, Black, Gray};
        public MeasurementSystem Measurement;
        public DotColor color;
        public List<string> ColorsAvailable;
        const int VERSION = 1;

        public MySettings()
        {
            Measurement = MeasurementSystem.Imperial;
            color = DotColor.Green;

            ColorsAvailable = new List<string>
            {
                "Red",
                "Green",
                "Blue",
                "Yellow",
                "Purple",
                "Orange",
                "Black",
                "Gray"
            };
        }

        public int IndexColors()
        {
            int temp = 0;
            switch (color)
            {
                case DotColor.Red:
                    temp = 0; break;

                case DotColor.Green:
                    temp = 1; break;

                case DotColor.Blue:
                    temp = 2; break;

                case DotColor.Yellow:
                    temp = 3; break;

                case DotColor.Purple:
                    temp = 4; break;

                case DotColor.Orange:
                    temp = 5; break;

                case DotColor.Black:
                    temp = 6; break;

                case DotColor.Gray:
                    temp = 7; break;
            }

            return temp;
        }

        public Color ReturnColor()
        { Color Temp = new Color();
            switch (color)
            {
                case DotColor.Green:
                    Temp = Color.Green; break;
                case DotColor.Red:
                    Temp = Color.Red; break;
                case DotColor.Gray:
                    Temp = Color.Gray; break;
                case DotColor.Blue:
                    Temp = Color.Blue; break;
                case DotColor.Black:
                    Temp = Color.Black; break;
                case DotColor.Orange:
                    Temp = Color.Orange; break;
                case DotColor.Purple:
                    Temp = Color.Indigo; break;
                case DotColor.Yellow:
                    Temp = Color.Yellow; break;
                default:
                    Temp = Color.Blue; break;
            }
            return Temp;
        }


        public DotColor List2DotColor(int Index)
        {
            DotColor temp= DotColor.Blue;
            switch (Index)
            {
                case 0:
                    temp = DotColor.Red; break;
                case 1:
                    temp = DotColor.Green; break;
                case 2:
                    temp = DotColor.Blue; break;
                case 3:
                    temp = DotColor.Yellow; break;
                case 4:
                    temp = DotColor.Purple; break;
                case 5:
                    temp = DotColor.Orange; break;
                case 6:
                    temp = DotColor.Black; break;
                case 7:
                    temp = DotColor.Gray; break;
                    
            }
            return temp;
        }
        public static void Save(MySettings set)
        {
            Stream stream = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "mcbsettings.mcb92"), FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, VERSION);
                formatter.Serialize(stream, set);

                System.Diagnostics.Debug.WriteLine("CREATED A SAVE FILE");
            }
            catch
            {
                // No Save File Created
                System.Diagnostics.Debug.WriteLine("DID NOT CREATE A SAVE FILE");
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }
        }

        public static MySettings Load()
        {
            Stream stream = null;
            MySettings Profile = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                stream = new FileStream(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "mcbsettings.mcb92"), FileMode.Open, FileAccess.Read, FileShare.None);
                int version = (int)formatter.Deserialize(stream);
                Profile = (MySettings)formatter.Deserialize(stream);
            }
            catch
            {
                // do nothing, just ignore any possible errors
                System.Diagnostics.Debug.WriteLine("DID NOT LOAD CORRECTLY");
            }
            finally
            {
                if (null != stream)
                    stream.Close();
            }
            return Profile;
        }
    }
}