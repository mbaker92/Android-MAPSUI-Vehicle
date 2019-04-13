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

        const int VERSION = 1;

        public MySettings()
        {
            Measurement = MeasurementSystem.Imperial;
            color = DotColor.Green;
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