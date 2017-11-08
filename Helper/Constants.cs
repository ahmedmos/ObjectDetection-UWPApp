using IntelligentKioskSample.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentKioskSample.Helper
{
    public static class Constants
    {

        //color ocmbinations
        public const string BlueColor = "#FF50b4da";
        public const string GreenColor = "#FF4dd5a1";
        public const string PeachColor = "#FFf28d79";
        public const string backgroundColorChat = "#FF2f3653";



        public const string FaceAPIKey = "";
        public const string EmotionApi = "";
        public const string ComputerVision = "";


        public static string x;
        public static string y;
        public static string l;
        public static string h;
        public static string op;
        public static string op2;

        public static List<ROIData> materials { get; set; }

        public static bool isHelmet
        {
            get; set;
        }

        public static bool isDashboard
        {
            get; set;
        }

        public static bool isFireDetected
        {
            get;set;
        }



    }
}
