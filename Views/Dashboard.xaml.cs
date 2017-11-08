using IntelligentKioskSample.Helper;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IntelligentKioskSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Dashboard : Page
    {
        public Dashboard()
        {
            this.InitializeComponent();

            indicator1.Fill = new SolidColorBrush(Colors.Gray);

        }
        //public Frame AppFrame { get { return this.frame; } }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Constants.isDashboard = true;
            await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: false);

            base.OnNavigatedTo(e);
        }
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await this.cameraControl.StopStreamAsync();
            Constants.isDashboard = false;
            base.OnNavigatingFrom(e);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RealTimeDemo), string.Empty, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
        }

        private void indicator1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VideoFlipView.SelectedIndex = 0;
            video1.Play();
            video2.Stop();
            video3.Stop();

            indicator1.Fill = new SolidColorBrush(Colors.Gray);
            indicator2.Fill = new SolidColorBrush(Colors.White);
            indicator3.Fill = new SolidColorBrush(Colors.White);

        }

        private void indicator2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VideoFlipView.SelectedIndex = 1;
            video1.Stop();
            video2.Play();
            video3.Stop();

            indicator1.Fill = new SolidColorBrush(Colors.White);
            indicator2.Fill = new SolidColorBrush(Colors.Gray);
            indicator3.Fill = new SolidColorBrush(Colors.White);
        }

        private void indicator3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VideoFlipView.SelectedIndex = 2;
            video1.Stop();
            video2.Stop();
            video3.Play();

            indicator1.Fill = new SolidColorBrush(Colors.White);
            indicator2.Fill = new SolidColorBrush(Colors.White);
            indicator3.Fill = new SolidColorBrush(Colors.Gray);
        }
    }
}
