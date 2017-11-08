using IntelligentKioskSample.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IntelligentKioskSample.Controls
{
    public sealed partial class ChatUserControl : UserControl
    {
        public ChatUserControl()
        {
            this.InitializeComponent();
          
             //   this.Height = 300 ;
        }
        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }
        public void setControlProperties(double height, string headingText, string messageText, string theme, BitmapImage image, bool isImage)
        {
            TimeText.Text = DateTime.Now.ToString("h:mm:ss");


            SolidColorBrush headingbackgroundcolor = GetSolidColorBrush(Constants.BlueColor);
            if (theme == "blue")
            {
                 headingbackgroundcolor = GetSolidColorBrush(Constants.BlueColor);
              //   startingtext.Text = "WARNING";
                charText.Text = "";
            }
            else
                if(theme == "green")
            {
                headingbackgroundcolor = GetSolidColorBrush(Constants.GreenColor);
               // startingtext.Text = "MESSAGE";
                charText.Text = "";
            }
            else
                if(theme == "peach")
            {
                headingbackgroundcolor = GetSolidColorBrush(Constants.PeachColor);
              //  startingtext.Text = "VIOLATION";
                charText.Text = "";
            }
          

            if(isImage)
            {
                DisplayImage.Source = image;
                DisplayImage.Height = 100;
                DisplayImage.Width = 200;
                DisplayImage.HorizontalAlignment = HorizontalAlignment.Center;
            }

           // this.Height = height;
            this.HeadingText.Text = headingText;
            this.MessageText.Text = messageText;

            MessageText.Foreground = headingbackgroundcolor;
            InnerGrid.Background = headingbackgroundcolor;

            //OuterGrid.Height = height;
            //OuterBorder.Height = height;

         
        }
    }
}
