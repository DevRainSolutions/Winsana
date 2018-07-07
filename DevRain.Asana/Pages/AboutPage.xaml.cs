using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.WP.Core.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;


namespace DevRain.Asana.Pages
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            tbVersion.Text = string.Format("Version: {0}", DeviceHelper.GetAppVersion().ToString());
        }

        private void BtnFeedback_OnClick(object sender, RoutedEventArgs e)
        {
            
            Tasks.ShowEmailComposeTask(AsanaConstants.FeedbackEmail,AsanaConstants.AppTitle);
        }

        private void BtnRate_OnClick(object sender, RoutedEventArgs e)
        {
            
            Tasks.ShowMarketplaceReviewTask();
        }

        private void BtnWebsite_OnClick(object sender, RoutedEventArgs e)
        {

            Tasks.ShowWebBrowserTask("http://devrain.com/");
        }

        private void BtnOtherapps_OnClick(object sender, RoutedEventArgs e)
        {
            Tasks.ShowMarketplaceSearchTask("devrain");
        }
    }
}