﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BugSense;
using DevRain.Asana.API;
using DevRain.Asana.API.Data;
using DevRain.Asana.Services;
using DevRain.WP.Core.Controls;
using DevRain.WP.Core.DevRainServices;
using DevRain.WP.Core.Helpers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;


namespace DevRain.Asana
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

	       // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            DevRainErrorHandler.Initialize(this, 10001);

#if !DEBUG
            BugSenseHandler.Instance.Init(this, "0763574c",new NotificationOptions(){Type=enNotificationType.None});
#endif

        }


    }
}